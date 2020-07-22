using Microsoft.AspNet.OData.Extensions;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.UriParser;
using System;
using System.Linq;
using Microsoft.OData.Edm;

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
    }
}
