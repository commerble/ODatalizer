using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Extensions;

namespace ODatalizer.EFCore.Routing.Conventions
{
    public class ODatalizerDynamicConvention : IODataControllerActionConvention
    {
        private readonly string _controllerName;
        public ODatalizerDynamicConvention(string controllerName)
        {
            _controllerName = controllerName;
        }

        public int Order => 9999;

        public bool AppliesToAction(ODataControllerActionContext context)
        {
            return true;
        }

        public bool AppliesToController(ODataControllerActionContext context)
        {
            return context.Controller.ControllerName == _controllerName;
        }

        //public IEnumerable<ControllerActionDescriptor> SelectAction(RouteContext routeContext)
        //{
        //    var actionCollectionProvider = routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
        //    return actionCollectionProvider
        //            .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
        //            .Where(c => c.ControllerName == _controllerName && c.ActionName.ToLower() == routeContext.HttpContext.Request.Method.ToLower());
        //}
    }
}
