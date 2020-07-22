using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OData.Edm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ODatalizer
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class EnableQueryRefAttribute : EnableQueryAttribute
    {
        private static ConcurrentDictionary<Type, Func<ActionExecutedContext, IEnumerable<object>, IEnumerable<Uri>>> _cachedUriResolvers 
            = new ConcurrentDictionary<Type, Func<ActionExecutedContext, IEnumerable<object>, IEnumerable<Uri>>>();
        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);

            HttpResponse response = actionExecutedContext.HttpContext.Response;

            // Check is the response is set and successful.
            if (response != null && IsSuccessStatusCode(response.StatusCode) && actionExecutedContext.Result != null)
            {
                // actionExecutedContext.Result might also indicate a status code that has not yet
                // been applied to the result; make sure it's also successful.
                var statusCodeResult = actionExecutedContext.Result as StatusCodeResult;
                if (statusCodeResult == null || IsSuccessStatusCode(statusCodeResult.StatusCode))
                {
                    ObjectResult responseContent = actionExecutedContext.Result as ObjectResult;
                    if (responseContent != null)
                    {
                        var results = responseContent.Value as IEnumerable<object>;
                        if (results == null)
                            return;

                        var first = results.FirstOrDefault();
                        if (first == null)
                            return;

                        var firstType = first.GetType();

                        var resolve = _cachedUriResolvers.GetOrAdd(firstType, type => {
                            var clrType = type.FullName.StartsWith("Castle.Proxies.") ? type.BaseType : 
                                          type.FullName.StartsWith("System.Data.Entity.DynamicProxies.") ? type.BaseType : type;
                            var model = actionExecutedContext.HttpContext.Request.GetModel();

                            var edmType = model.FindDeclaredType(clrType.FullName) as EdmEntityType;
                            if (edmType == null)
                                return null;

                            var entitySet = model.EntityContainer.Elements.FirstOrDefault(el => el.ContainerElementKind == EdmContainerElementKind.EntitySet && ((EdmEntitySet)el).EntityType() == edmType);
                            if (entitySet == null)
                                return null;

                            var keySelectors = edmType.DeclaredKey.Select<IEdmStructuralProperty, Func<object, KeyValuePair<string, object>>>(key =>
                            {
                                var propInfo = clrType.GetProperty(key.Name);
                                return o => KeyValuePair.Create(key.Name, propInfo.GetValue(o));
                            }).ToList();

                            return (context, values) => context.HttpContext.Request.ResolveResourceUris(entitySet.Name, values.Select(o => keySelectors.Select(f => f(o))));
                        });

                        if (resolve == null)
                            return;
                        
                        responseContent.Value = resolve(actionExecutedContext, results);
                    }
                }
            }
        }

        private static bool IsSuccessStatusCode(int statusCode)
        {
            return statusCode >= 200 && statusCode < 300;
        }
    }
}
