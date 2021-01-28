using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ODatalizer
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class EnableQueryRefAttribute : EnableQueryAttribute
    {
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

                        var resolve = actionExecutedContext.HttpContext.Request.GetResourceUriResolver(firstType);

                        if (resolve == null)
                            return;
                        
                        responseContent.Value = resolve(results);
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
