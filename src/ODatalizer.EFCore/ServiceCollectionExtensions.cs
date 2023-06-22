using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.NewtonsoftJson;
using Microsoft.Extensions.DependencyInjection;
using ODatalizer.Batch;
using ODatalizer.EFCore.Builders;
using ODatalizer.EFCore.Routing.Conventions;
using System;

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
                        });
                    }
                })
                .AddODataNewtonsoftJson();
                
        }
    }
}
