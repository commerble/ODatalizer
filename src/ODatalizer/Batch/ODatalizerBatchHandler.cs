using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace ODatalizer.Batch
{
    public class ODatalizerBatchHandler : DefaultODataBatchHandler
    {
        public override async Task<IList<ODataBatchResponseItem>> ExecuteRequestMessagesAsync(IEnumerable<ODataBatchRequestItem> requests, RequestDelegate handler)
        {
            var tran = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled);

            var responses = await base.ExecuteRequestMessagesAsync(requests, handler);

            if (responses.All(r => {
                if (r is ChangeSetResponseItem c)
                    return c.Contexts.All(c => c.Response.IsSuccessStatusCode());
                if (r is OperationResponseItem o)
                    return o.Context.Response.IsSuccessStatusCode();

                return false;
            }))
            {
                tran.Complete();
            }

            tran.Dispose();

            return responses;
        }
        
        public override async Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            HttpRequest request = context.Request;
            IServiceProvider requestContainer = request.CreateRequestContainer(ODataRouteName);
            requestContainer.GetRequiredService<ODataMessageReaderSettings>().BaseUri = GetBaseUri(request);

            ODataMessageReader reader = request.GetODataMessageReader(requestContainer);

            CancellationToken cancellationToken = context.RequestAborted;
            List<ODataBatchRequestItem> requests = new List<ODataBatchRequestItem>();
            ODataBatchReader batchReader = await reader.CreateODataBatchReaderAsync();
            Guid batchId = Guid.NewGuid();
            while (await batchReader.ReadAsync())
            {
                if (batchReader.State == ODataBatchReaderState.ChangesetStart)
                {
                    IList<HttpContext> changeSetContexts = await ReadChangeSetRequestAsync(batchReader, context, batchId, cancellationToken);
                    foreach (HttpContext changeSetContext in changeSetContexts)
                    {
                        changeSetContext.Request.CopyBatchRequestProperties(request);
                        changeSetContext.Request.DeleteRequestContainer(false);
                    }
                    requests.Add(new ChangeSetRequestItem(changeSetContexts));
                }
                else if (batchReader.State == ODataBatchReaderState.Operation)
                {
                    HttpContext operationContext = await batchReader.ReadOperationRequestAsync(context, batchId, true, cancellationToken);
                    operationContext.Request.CopyBatchRequestProperties(request);
                    operationContext.Request.DeleteRequestContainer(false);
                    requests.Add(new OperationRequestItem(operationContext));
                }
            }

            return requests;
        }
        
        public async Task<IList<HttpContext>> ReadChangeSetRequestAsync(ODataBatchReader reader, HttpContext context, Guid batchId, CancellationToken cancellationToken)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.State != ODataBatchReaderState.ChangesetStart)
            {
                throw new InvalidOperationException(reader.State.ToString() + ODataBatchReaderState.ChangesetStart.ToString());
            } 

            Guid changeSetId = Guid.NewGuid();
            List<HttpContext> contexts = new List<HttpContext>();
            while (await reader.ReadAsync() && reader.State != ODataBatchReaderState.ChangesetEnd)
            {
                if (reader.State == ODataBatchReaderState.Operation)
                {
                    contexts.Add(await ReadOperationInternalAsync(reader, context, batchId, changeSetId, cancellationToken));
                }
            }

            return contexts;
        }
        
        private static readonly string[] nonInheritableHeaders = new string[] { "content-length", "content-type" };
        private static readonly string[] nonInheritablePreferences = new string[] { "respond-async", "continue-on-error", "odata.continue-on-error" };

        static async Task<HttpContext> ReadOperationInternalAsync(
            ODataBatchReader reader, HttpContext originalContext, Guid batchId, Guid? changeSetId, CancellationToken cancellationToken, bool bufferContentStream = true)
        {
            ODataBatchOperationRequestMessage batchRequest = await reader.CreateOperationRequestMessageAsync();

            HttpContext context = CreateHttpContext(originalContext);
            HttpRequest request = context.Request;

            request.Method = batchRequest.Method;
            if (batchRequest.Url.IsAbsoluteUri)
            {
                request.CopyAbsoluteUrl(batchRequest.Url);
            }
            else
            {
                var pathAndQuery = batchRequest.Url.OriginalString.Split('?');
                var path = new PathString(pathAndQuery[0]);
                if (path.StartsWithSegments(request.PathBase, out PathString remainingPath))
                {
                    path = remainingPath;
                }
                request.Path = path;

                if (pathAndQuery.Length > 1)
                {
                    request.QueryString = new QueryString("?" + pathAndQuery[1]);
                }
            }
            
            

            // Not using bufferContentStream. Unlike AspNet, AspNetCore cannot guarantee the disposal
            // of the stream in the context of execution so there is no choice but to copy the stream
            // from the batch reader.
            using (Stream stream = await batchRequest.GetStreamAsync())
            {
                MemoryStream bufferedStream = new MemoryStream();
                // Passing in the default buffer size of 81920 so that we can also pass in a cancellation token
                await stream.CopyToAsync(bufferedStream, bufferSize: 81920, cancellationToken: cancellationToken);
                bufferedStream.Position = 0;
                request.Body = bufferedStream;
            }

            foreach (var header in batchRequest.Headers)
            {
                string headerName = header.Key;
                string headerValue = header.Value;

                if (headerName.Trim().ToLowerInvariant() == "prefer")
                {
                    // in the case of Prefer header, we don't want to overwrite,
                    // instead we merge preferences defined in the individual request with those inherited from the batch
                    request.Headers.TryGetValue(headerName, out StringValues batchReferences);
                    request.Headers[headerName] = MergeIndividualAndBatchPreferences(headerValue, batchReferences);
                }
                else
                {
                    // Copy headers from batch, overwriting any existing headers.
                    request.Headers[headerName] = headerValue;
                }
            }

            request.SetODataBatchId(batchId);
            request.SetODataContentId(batchRequest.ContentId);

            if (changeSetId != null && changeSetId.HasValue)
            {
                request.SetODataChangeSetId(changeSetId.Value);
            }

            return context;
        }

        private static HttpContext CreateHttpContext(HttpContext originalContext)
        {
            // Clone the features so that a new set is used for each context.
            // The features themselves will be reused but not the collection. We
            // store the request container as a feature of the request and we don't want
            // the features added to one context/request to be visible on another.
            //
            // Note that just about everything inm the HttpContext and HttpRequest is
            // backed by one of these features. So reusing the features means the HttContext
            // and HttpRequests are the same without needing to copy properties. To make them
            // different, we need to avoid copying certain features to that the objects don't
            // share the same storage/
            IFeatureCollection features = new FeatureCollection();
            string pathBase = "";
            foreach (KeyValuePair<Type, object> kvp in originalContext.Features)
            {
                // Don't include the OData features. They may already
                // be present. This will get re-created later.
                //
                // Also, clear out the items feature, which is used
                // to store a few object, the one that is an issue here is the Url
                // helper, which has an affinity to the context. If we leave it,
                // the context of the helper no longer matches the new context and
                // the resulting url helper doesn't have access to the OData feature
                // because it's looking in the wrong context.
                //
                // Because we need a different request and response, leave those features
                // out as well.
                if (kvp.Key == typeof(IHttpRequestFeature))
                {
                    pathBase = ((IHttpRequestFeature)kvp.Value).PathBase;
                }

                if (kvp.Key == typeof(IODataBatchFeature) ||
                    kvp.Key == typeof(IODataFeature) ||
                    kvp.Key == typeof(IItemsFeature) ||
                    kvp.Key == typeof(IHttpRequestFeature) ||
                    kvp.Key == typeof(IHttpResponseFeature))
                {
                    continue;
                }

#if !NETSTANDARD2_0
                if (kvp.Key == typeof(IEndpointFeature))
                {
                    continue;
                }
#endif

                features[kvp.Key] = kvp.Value;
            }

            // Add in an items, request and response feature.
            features[typeof(IItemsFeature)] = new ItemsFeature();
            features[typeof(IHttpRequestFeature)] = new HttpRequestFeature
            {
                PathBase = pathBase
            };

            features[typeof(IHttpResponseFeature)] = new HttpResponseFeature();

            // Create a context from the factory or use the default context.
            HttpContext context = null;
            IHttpContextFactory httpContextFactory = originalContext.RequestServices.GetRequiredService<IHttpContextFactory>();
            if (httpContextFactory != null)
            {
                context = httpContextFactory.Create(features);
            }
            else
            {
                context = new DefaultHttpContext(features);
            }

            // Clone parts of the request. All other parts of the request will be 
            // populated during batch processing.
            context.Request.Cookies = originalContext.Request.Cookies;
            foreach (KeyValuePair<string, StringValues> header in originalContext.Request.Headers)
            {
                string headerKey = header.Key.ToLowerInvariant();
                // do not copy over headers that should not be inherited from batch to individual requests
                if (!nonInheritableHeaders.Contains(headerKey))
                {
                    // some preferences may be inherited, others discarded
                    if (headerKey == "prefer")
                    {
                        string preferencesToInherit = GetPreferencesToInheritFromBatch(header.Value);
                        if (!string.IsNullOrEmpty(preferencesToInherit))
                        {
                            context.Request.Headers.Add(header.Key, preferencesToInherit);
                        }
                    }
                    else
                    {
                        context.Request.Headers.Add(header);
                    }
                }
            }

            // Create a response body as the default response feature does not
            // have a valid stream.
            // Use a special batch stream that remains open after the writer is disposed.
            context.Response.Body = new ODataBatchStream();

            return context;
        }

        private static string GetPreferencesToInheritFromBatch(string batchPreferences)
        {
            IEnumerable<string> preferencesToInherit = SplitPreferences(batchPreferences)
                .Where(value =>
                    !nonInheritablePreferences.Any(
                        prefToIgnore =>
                        value.ToLowerInvariant().StartsWith(prefToIgnore)
                    )
                );
            return string.Join(",", preferencesToInherit);
        }

        private static string MergeIndividualAndBatchPreferences(string individualPreferences, string batchPreferences)
        {
            if (string.IsNullOrEmpty(individualPreferences))
            {
                return batchPreferences;
            }

            if (string.IsNullOrEmpty(batchPreferences))
            {
                return individualPreferences;
            }
            // get the name of each preference to avoid adding duplicates from batch
            IEnumerable<string> individualList = SplitPreferences(individualPreferences);
            HashSet<string> individualPreferenceNames = new HashSet<string>(individualList.Select(pref => pref.Split('=').FirstOrDefault()));


            IEnumerable<string> filteredBatchList = SplitPreferences(batchPreferences)
                // do not add duplicate preferences from batch
                .Where(pref => !individualPreferenceNames.Contains(pref.Split('=').FirstOrDefault()));
            string filteredBatchPreferences = string.Join(",", filteredBatchList);

            return string.Join(",", individualPreferences, filteredBatchPreferences);
        }
        private static IEnumerable<string> SplitPreferences(string preferences)
        {
            int preferenceStartIndex = 0;

            HashSet<string> addedPreferences = new HashSet<string>();
            bool insideQuotedValue = false;
            for (int currentIndex = 0; currentIndex < preferences.Length; currentIndex++)
            {
                char c = preferences[currentIndex];
                if (c == '"')
                {
                    if (!insideQuotedValue)
                    {
                        // we are starting a double-quoted value
                        insideQuotedValue = true;
                    }
                    else
                    {
                        // this could be the end of a quoted value, or it could be an escaped quote
                        // we're sure that currentIndex > 0 here since insideQuotedValue is true, so need to check for bounds
                        insideQuotedValue = preferences[currentIndex - 1] == '\\';
                    }
                }
                else if (c == ',' && !insideQuotedValue)
                {
                    string result = preferences.Substring(preferenceStartIndex, currentIndex - preferenceStartIndex).Trim();
                    string prefName = result.Split('=')[0].Trim();
                    // do not add duplicate preference
                    if (!addedPreferences.Contains(prefName))
                    {
                        addedPreferences.Add(prefName);
                        yield return result;
                    }

                    preferenceStartIndex = currentIndex + 1;
                }
            }

            if (preferences.Length > preferenceStartIndex + 1)
            {
                yield return preferences.Substring(preferenceStartIndex).Trim();
            }
        }

        internal class ODataBatchStream : MemoryStream
        {
            private bool isDisposed = false;

            /// <summary>
            /// Dispose the batch stream and underlying resources
            /// </summary>
            internal void InternalDispose()
            {
                if (!isDisposed)
                {
                    base.Flush();
                    base.Close();
                    base.Dispose();
                    isDisposed = true;
                }
            }

            /// <summary>
            /// Dispose the batch stream and underlying resources
            /// </summary>
            internal async Task InternalDisposeAsync()
            {
                if (!isDisposed)
                {
                    await base.FlushAsync();
                    base.Close();
                    base.Dispose();
                    isDisposed = true;
                }
            }

            /// <summary>
            /// Override Close() in order to hold the stream open until we are able to
            /// copy it to the batch response stream.
            /// </summary>
            public override void Close()
            {
            }
        }
    }
}
