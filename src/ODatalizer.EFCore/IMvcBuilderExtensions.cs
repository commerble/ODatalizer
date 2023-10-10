using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.NewtonsoftJson;
using Microsoft.Extensions.DependencyInjection;
using ODatalizer.EFCore.Batch;
using ODatalizer.EFCore.Builders;
using ODatalizer.EFCore.Routing;
using System;
using System.Collections.Generic;

namespace ODatalizer.EFCore
{
    public static class IMvcBuilderExtensions
    {
        public static IMvcBuilder AddODatalizer(this IMvcBuilder builder, Func<IServiceProvider, IEnumerable<ODatalizerEndpoint>> endpointsFactory)
        {
            return builder.AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            })
            .AddOData((opt, sp) =>
            {
                sp = sp.CreateScope().ServiceProvider;

                var endpoints = endpointsFactory.Invoke(sp);

                AddODatalizerControllers(sp, endpoints);

                opt
                    .Select()
                    .Expand()
                    .Filter()
                    .OrderBy()
                    .SetMaxTop(ODatalizerEndpoint.DefaultPageSize)
                    .Count()
                    .SkipToken()
                    .EnableQueryFeatures();
                foreach (var ep in endpoints)
                {
                    opt.AddRouteComponents(ep.RoutePrefix, ep.EdmModel, services =>
                    {
                        services.AddSingleton<ODataBatchHandler, ODatalizerBatchHandler>();
                        if (ep.ODatalizerController != null)
                        {
                            services.AddSingleton(new ODatalizerControllerNameAccessor(ep.ODatalizerController.Replace("Controller", string.Empty)));
                        }
                    });
                }
            })
            .AddODataNewtonsoftJson();
        }
        private static void AddODatalizerControllers(IServiceProvider sp, IEnumerable<ODatalizerEndpoint> endpoints)
        {
            var controllerBuilder = sp.GetRequiredService<ControllerBuilder>();
            var part = sp.GetRequiredService<ApplicationPartManager>();
            foreach (var ep in endpoints)
            {
                var assembly = controllerBuilder.Build(ep);
                part.ApplicationParts.Add(new AssemblyPart(assembly));
            }
        }
    }
}
