using Microsoft.AspNet.OData.Extensions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.UriParser;
using System;
using System.Linq;
using Microsoft.OData.Edm;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNet.OData;
using Microsoft.OData;

namespace ODatalizer
{
    public static class HttpRequestMessageExtensions
    {
        public static string GetServiceRoot(this HttpRequest request)
        {
            var urlHelper = request.GetUrlHelper();
            return urlHelper.CreateODataLink();
        }

        public static IEnumerable<IReadOnlyDictionary<string, object>> GetKeysFromUri(this HttpRequest request, Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            var serviceRoot = GetServiceRoot(request);
            var serviceRootUri = serviceRoot.EndsWith("/") ? new Uri(serviceRoot) : new Uri(serviceRoot + "/");
            var pathHandler = request.GetPathHandler();
            var odataPath = pathHandler.Parse(serviceRoot, serviceRootUri.MakeRelativeUri(uri).ToString(), request.GetRequestContainer());

            foreach (var keySegment in odataPath.Segments.OfType<KeySegment>())
            {
                yield return keySegment.Keys.ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

        public static (string, IReadOnlyDictionary<string, object>) GetEntitySetAndKeysFromUri(this HttpRequest request, Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            var serviceRoot = GetServiceRoot(request);
            var serviceRootUri = serviceRoot.EndsWith("/") ? new Uri(serviceRoot) : new Uri(serviceRoot + "/");
            var pathHandler = request.GetPathHandler();
            var odataPath = pathHandler.Parse(serviceRoot, serviceRootUri.MakeRelativeUri(uri).ToString(), request.GetRequestContainer());
            var entitySetSegment = odataPath.Segments.OfType<EntitySetSegment>().First();
            var keySegment = odataPath.Segments.OfType<KeySegment>().First();

            return (entitySetSegment.EntitySet.Name, keySegment.Keys.ToDictionary(kv => kv.Key, kv => kv.Value));
        }

        public static IEnumerable<Uri> ResolveResourceUris(this HttpRequest request, string entitySetName, IEnumerable<KeyValuePair<string, object>> keys)
            => ResolveResourceUris(request, entitySetName, keys.Select(_ => new[] { _ }));
        public static IEnumerable<Uri> ResolveResourceUris(this HttpRequest request, string entitySetName, IEnumerable<IEnumerable<KeyValuePair<string, object>>> keys)
        {
            var urlHelper = request.GetUrlHelper();

            var edm = request.GetModel();
            var entitySet = edm.EntityContainer.FindEntitySet(entitySetName);
            var entitySetSegment = new EntitySetSegment(entitySet);

            foreach (var key in keys)
            {
                var keySegment = new KeySegment(key, entitySet.EntityType(), null);
                var uri = urlHelper.CreateODataLink(entitySetSegment, keySegment);
                yield return new Uri(uri);
            }
        }

        private static ConcurrentDictionary<Type, Func<IEnumerable<object>, IEnumerable<Uri>>> _cachedUriResolvers
            = new ConcurrentDictionary<Type, Func<IEnumerable<object>, IEnumerable<Uri>>>();

        public static Func<IEnumerable<object>, IEnumerable<Uri>> GetResourceUriResolver(this HttpRequest request, Type type)
        {
            var resolver = _cachedUriResolvers.GetOrAdd(type, type =>
            {
                var clrType = type.FullName.StartsWith("Castle.Proxies.") ? type.BaseType
                            : type.FullName.StartsWith("System.Data.Entity.DynamicProxies.") ? type.BaseType
                            : type;

                var edm = request.GetModel();

                var edmType = edm.FindDeclaredType(clrType.FullName) as EdmEntityType;
                if (edmType == null)
                    return null;

                var entitySet = edm.EntityContainer.Elements.FirstOrDefault(el => el.ContainerElementKind == EdmContainerElementKind.EntitySet && ((EdmEntitySet)el).EntityType() == edmType) as EdmEntitySet;
                if (entitySet == null)
                    return null;

                var keySelectors = edmType.DeclaredKey.Select<IEdmStructuralProperty, Func<object, KeyValuePair<string, object>>>(key =>
                {
                    var propInfo = clrType.GetProperty(key.Name);
                    return o => KeyValuePair.Create(key.Name, propInfo.GetValue(o));
                }).ToList();

                var entitySetSegment = new EntitySetSegment(entitySet);
                var urlHelper = request.GetUrlHelper();

                return (values) =>
                    values.Select(o => new KeySegment(keySelectors.Select(f => f(o)), entitySet.EntityType(), null))
                          .Select(keySegment => new Uri(urlHelper.CreateODataLink(entitySetSegment, keySegment)));
            });
            return resolver;
        }

        public static SerializableError CreateSerializableErrorFromModelState(this ODataController controller)
        {
            // https://source.dot.net/#Microsoft.AspNetCore.Mvc.Core/SerializableError.cs,19bc9a1c61ce7ae0
            var serializableError = new SerializableError();

            foreach (var keyModelStatePair in controller.ModelState)
            {
                var key = keyModelStatePair.Key;
                var errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    var errorMessages = errors.Select(error =>
                    {
                        return string.IsNullOrEmpty(error.ErrorMessage) ? "The input was not valid." : error.ErrorMessage;
                    }).ToArray();
                    
                    serializableError.Add(key, errorMessages);

                    foreach (var error in errors)
                    {
                        if (error.Exception != null)
                        {
                            // Add more error details.
                            // e.g.
                            // ```
                            // One or more errors occurred. (
                            //  One or more errors occurred. (
                            //   A null value was found for the property named 'UnitPrice', which has the expected type 'Edm.Decimal[Nullable=False]'. 
                            //   The expected type 'Edm.Decimal[Nullable=False]' does not allow null values.
                            //  )
                            // )
                            // ```
                            serializableError.Add("MessageDetail", error.Exception.Message);
                        }
                    }

                }
            }

            return serializableError;
        }
    }
}
