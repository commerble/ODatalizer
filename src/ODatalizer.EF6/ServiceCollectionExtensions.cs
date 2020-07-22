using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ODatalizer.EF6.Builders;

namespace ODatalizer.EF6
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
