using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace ODatalizer.EFCore.Routing
{
    public class ODatalizerRoutingApplciationModelProvider : IApplicationModelProvider
    {
        private readonly IOptions<ODataOptions> _options;

        public ODatalizerRoutingApplciationModelProvider(IOptions<ODataOptions> options)
        {
            _options = options;
        }
        public int Order => 90;

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            foreach (var controllerModel in context.Result.Controllers)
            {
                var routeComponent = _options.Value.RouteComponents.FirstOrDefault(kv => 
                                            kv.Value.ServiceProvider.GetService<ODatalizerControllerNameAccessor>()?.ControllerName == controllerModel.ControllerName);
                if (routeComponent.Value.EdmModel != null)
                {
                    var prefix = routeComponent.Key;
                    var model = _options.Value.RouteComponents[prefix].EdmModel;
                    foreach (var actionModel in controllerModel.Actions)
                    {
                        if (_dynamicMethodNames.Contains(actionModel.ActionName))
                        {
                            AddOrUpdateSelector(actionModel, actionModel.ActionName.ToUpperInvariant(), prefix, model);
                        }
                    }
                }
            }
        }

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
        }

        private static void AddRange<T>(IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        private static readonly string[] _dynamicMethodNames = new[] { "Get", "Post", "Put", "Patch", "Delete" }; 
        
        private static void AddSelector(ActionModel actionModel, string[] httpMethods, bool acceptPreflight)
        {
            var selectorModel = new SelectorModel();
            IReadOnlyList<object> attributes = actionModel.Attributes;

            AddRange(selectorModel.ActionConstraints, attributes.OfType<IActionConstraintMetadata>());

            for (var i = selectorModel.ActionConstraints.Count - 1; i >= 0; i--)
            {
                if (selectorModel.ActionConstraints[i] is IRouteTemplateProvider)
                {
                    selectorModel.ActionConstraints.RemoveAt(i);
                }
            }

            for (var i = selectorModel.ActionConstraints.Count - 1; i >= 0; i--)
            {
                if (selectorModel.ActionConstraints[i] is HttpMethodActionConstraint)
                {
                    selectorModel.ActionConstraints.RemoveAt(i);
                }
            }

            AddRange(selectorModel.EndpointMetadata, attributes);
            for (var i = selectorModel.EndpointMetadata.Count - 1; i >= 0; i--)
            {
                if (selectorModel.EndpointMetadata[i] is IRouteTemplateProvider)
                {
                    selectorModel.EndpointMetadata.RemoveAt(i);
                }
            }

            for (var i = selectorModel.EndpointMetadata.Count - 1; i >= 0; i--)
            {
                if (selectorModel.EndpointMetadata[i] is IHttpMethodMetadata)
                {
                    selectorModel.EndpointMetadata.RemoveAt(i);
                }
            }
            selectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(httpMethods));
            selectorModel.EndpointMetadata.Add(new HttpMethodMetadata(httpMethods, acceptPreflight));
            actionModel.Selectors.Add(selectorModel);
        }

        private static void UpdateSelector(SelectorModel selectorModel, string[] httpMethods, bool acceptPreflight)
        {
            // remove the unused constraints (just for safe)
            for (var i = selectorModel.ActionConstraints.Count - 1; i >= 0; i--)
            {
                if (selectorModel.ActionConstraints[i] is IRouteTemplateProvider)
                {
                    selectorModel.ActionConstraints.RemoveAt(i);
                }
            }

            for (var i = selectorModel.ActionConstraints.Count - 1; i >= 0; i--)
            {
                if (selectorModel.ActionConstraints[i] is HttpMethodActionConstraint)
                {
                    selectorModel.ActionConstraints.RemoveAt(i);
                }
            }

            // remove the unused metadata
            for (var i = selectorModel.EndpointMetadata.Count - 1; i >= 0; i--)
            {
                if (selectorModel.EndpointMetadata[i] is IRouteTemplateProvider)
                {
                    selectorModel.EndpointMetadata.RemoveAt(i);
                }
            }

            for (var i = selectorModel.EndpointMetadata.Count - 1; i >= 0; i--)
            {
                if (selectorModel.EndpointMetadata[i] is IHttpMethodMetadata)
                {
                    selectorModel.EndpointMetadata.RemoveAt(i);
                }
            }

            selectorModel.ActionConstraints.Add(new HttpMethodActionConstraint(httpMethods));
            selectorModel.EndpointMetadata.Add(new HttpMethodMetadata(httpMethods, acceptPreflight));
        }

        private static void AddOrUpdateSelector(ActionModel actionModel, string httpMethod, string routePrefix, IEdmModel model)
        {
            var acceptPreflight =
                actionModel.Controller.Attributes.OfType<IDisableCorsAttribute>().Any() ||
                actionModel.Controller.Attributes.OfType<IEnableCorsAttribute>().Any() ||
                actionModel.Attributes.OfType<IDisableCorsAttribute>().Any() ||
                actionModel.Attributes.OfType<IEnableCorsAttribute>().Any();

            var selectorModel = actionModel.Selectors.FirstOrDefault(s => s.AttributeRouteModel == null);
            if (selectorModel == null)
            {
                AddSelector(actionModel, new[] { httpMethod }, acceptPreflight);
            }
            else
            {
                UpdateSelector(selectorModel, new[] { httpMethod }, acceptPreflight);
            }

            selectorModel.EndpointMetadata.Add(new ODatalizerRoutingMetadata(routePrefix, model));
            selectorModel.AttributeRouteModel = new AttributeRouteModel
            {
                Template = $"/{routePrefix}/{{*path}}",
                Name = $"{httpMethod} /{routePrefix}/{{*path}}"
            };
        }
    }
}
