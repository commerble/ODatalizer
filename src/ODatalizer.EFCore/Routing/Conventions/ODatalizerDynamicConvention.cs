using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace ODatalizer.EFCore.Routing.Conventions
{
    public class ODatalizerDynamicConvention : IODataRoutingConvention
    {
        private readonly string _controllerName;
        public ODatalizerDynamicConvention(string controllerName)
        {
            _controllerName = controllerName;
        }
        public IEnumerable<ControllerActionDescriptor> SelectAction(RouteContext routeContext)
        {
            var actionCollectionProvider = routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
            return actionCollectionProvider
                    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Where(c => c.ControllerName == _controllerName && c.ActionName.ToLower() == routeContext.HttpContext.Request.Method.ToLower());
        }
    }
}
