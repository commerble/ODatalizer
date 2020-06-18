using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ODatalizer.EFCore.Builders;

namespace ODatalizer.EFCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddODatalizer(this IServiceCollection services)
        {
            services.AddSingleton<ControllerBuilder>();
            services.AddOData();
        }
    }
}
