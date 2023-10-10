using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ODatalizer.EFCore.Builders;
using ODatalizer.EFCore.Routing;
using System;
using System.Collections.Generic;

namespace ODatalizer.EFCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddODatalizer(this IServiceCollection services, Func<IServiceProvider, IEnumerable<ODatalizerEndpoint>> endpointsFactory)
        {
            services.AddODatalizer(s => s.AddControllers(options => options.AddODatalizerOptions()), endpointsFactory);
        }

        public static void AddODatalizer(this IServiceCollection services, Func<IServiceCollection, IMvcBuilder> initMvcBuilder, Func<IServiceProvider, IEnumerable<ODatalizerEndpoint>> endpointsFactory)
        {
            services.AddSingleton<ControllerBuilder>();
            initMvcBuilder(services).AddODatalizer(endpointsFactory);
            services.TryAddEnumerable(ServiceDescriptor.Transient<IApplicationModelProvider, ODatalizerRoutingApplciationModelProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, ODatalizerRoutingMatcherPolicy>());
        }
    }
}
