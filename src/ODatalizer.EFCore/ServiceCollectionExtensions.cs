using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.NewtonsoftJson;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ODatalizer.EFCore.Batch;
using ODatalizer.EFCore.Builders;
using ODatalizer.EFCore.Converters;
using ODatalizer.EFCore.Routing;
using System;
using System.Collections.Generic;

namespace ODatalizer.EFCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddODatalizer(this IServiceCollection services, Func<IServiceProvider, ODatalizerEndpoint[]> endpointsFactory)
        {
            services.AddSingleton<ControllerBuilder>();
            services.AddControllers()
                .AddNewtonsoftJson(opt =>
                {
                    opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                })
                .AddOData((opt, sp) =>
                {
                    var endpoints = endpointsFactory.Invoke(sp.CreateScope().ServiceProvider);
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
                            services.AddSingleton(new ODatalizerControllerNameAccessor(ep.ODatalizerController.Replace("Controller", string.Empty)));
                        });
                    }
                })
                .AddODataNewtonsoftJson();

            services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, ODatalizerRoutingApplciationModelProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, ODatalizerRoutingMatcherPolicy>());
        }
    }
    public static class MvcOptionsExtensions
    {
        public static void AddODatalizerOptions(this MvcOptions options)
        {
            options.ModelBinderProviders.Insert(0, new ODatalizerModelBinderProvider());
        }
    }
}
