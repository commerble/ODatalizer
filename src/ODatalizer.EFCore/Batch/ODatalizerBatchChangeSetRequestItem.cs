using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODatalizer.EFCore.Batch
{
    public class ODatalizerBatchChangeSetRequestItem : ChangeSetRequestItem
    {
        public ODatalizerBatchChangeSetRequestItem(IEnumerable<HttpContext> contexts) : base(contexts)
        {
        }

        public override async Task<ODataBatchResponseItem> SendRequestAsync(RequestDelegate handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();
            List<HttpContext> responseContexts = new List<HttpContext>();

            foreach (HttpContext context in Contexts)
            {
                await SendRequestAsync(handler, context, contentIdToLocationMapping);

                HttpResponse response = context.Response;
                if (response.IsSuccessStatusCode())
                {
                    responseContexts.Add(context);
                }
                else
                {
                    responseContexts.Clear();
                    responseContexts.Add(context);
                    return new ChangeSetResponseItem(responseContexts);
                }
            }

            return new ChangeSetResponseItem(responseContexts);
        }

        public new static async Task SendRequestAsync(RequestDelegate handler, HttpContext context, Dictionary<string, string> contentIdToLocationMapping)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (contentIdToLocationMapping != null)
            {
                string path = context.Request.Path.HasValue ? context.Request.Path.Value : string.Empty;
                string resolvedRequestUrl = ResolveContentId(path, contentIdToLocationMapping);
                if (!string.IsNullOrEmpty(resolvedRequestUrl))
                {
                    Uri resolvedUri = new Uri(resolvedRequestUrl, UriKind.RelativeOrAbsolute);
                    if (resolvedUri.IsAbsoluteUri)
                    {
                        context.Request.CopyAbsoluteUrl(resolvedUri);
                    }
                    else
                    {
                        context.Request.Path = new PathString(resolvedRequestUrl);
                    }
                }

                context.Request.SetODataContentIdMapping(contentIdToLocationMapping);
            }

            try
            {
                await handler(context);

                string contentId = context.Request.GetODataContentId();

                if (contentIdToLocationMapping != null && contentId != null)
                {
                    AddLocationHeaderToMapping(context.Response, contentIdToLocationMapping, contentId);
                }
            }
            catch (Exception ex)
            {
                // Unlike AspNet, the exception handling is (by default) upstream of this middleware
                // so we need to trap exceptions on our own. This code is similar to the
                // ExceptionHandlerMiddleware class in AspNetCore.
                context.Response.Clear();
                context.Response.StatusCode = 500;
            }
        }

        private static void AddLocationHeaderToMapping(
            HttpResponse response,
            IDictionary<string, string> contentIdToLocationMapping,
            string contentId)
        {
            if (response?.Headers == null)
                throw new ArgumentNullException(nameof(response));

            if (contentIdToLocationMapping == null)
                throw new ArgumentNullException(nameof(contentIdToLocationMapping));

            if (contentId == null)
                throw new ArgumentNullException(nameof(contentId));

            var headers = response.GetTypedHeaders();
            if (headers.Location != null)
            {
                contentIdToLocationMapping.Add(contentId, headers.Location.AbsoluteUri);
            }
        }

        public static string ResolveContentId(string url, IDictionary<string, string> contentIdToLocationMapping)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            if (contentIdToLocationMapping == null)
                throw new ArgumentNullException(nameof(contentIdToLocationMapping));

            int startIndex = 0;

            while (true)
            {
                startIndex = url.IndexOf('$', startIndex);

                if (startIndex == -1)
                {
                    break;
                }

                int keyLength = 0;

                while (startIndex + keyLength < url.Length - 1 && IsContentIdCharacter(url[startIndex + keyLength + 1]))
                {
                    keyLength++;
                }

                if (keyLength > 0)
                {
                    // Might have matched a $<content-id> alias.
                    string locationKey = url.Substring(startIndex + 1, keyLength);
                    string locationValue;

                    if (contentIdToLocationMapping.TryGetValue(locationKey, out locationValue))
                    {
                        // As location headers MUST be absolute URL's, we can ignore everything 
                        // before the $content-id while resolving it.
                        return locationValue + url.Substring(startIndex + 1 + keyLength);
                    }
                }

                startIndex++;
            }

            return url;
        }

        private static bool IsContentIdCharacter(char c)
        {
            // According to the OData ABNF grammar, Content-IDs follow the scheme.
            // content-id = "Content-ID" ":" OWS 1*unreserved
            // unreserved    = ALPHA / DIGIT / "-" / "." / "_" / "~"
            switch (c)
            {
                case '-':
                case '.':
                case '_':
                case '~':
                    return true;
                default:
                    return Char.IsLetterOrDigit(c);
            }
        }
    }
}
