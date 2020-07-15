using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using ODatalizer.EFCore.Builders;

namespace ODatalizer.EFCore
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseODatalizer(this IApplicationBuilder app, params ODatalizerEndpoint[] endpoints)
        {
            app.UseODatalizerControllers(endpoints);
            app.UseODataBatching();
        }

        public static void UseODatalizerControllers(this IApplicationBuilder app, params ODatalizerEndpoint[] endpoints)
        {
            var controllerBuilder = app.ApplicationServices.GetRequiredService<ControllerBuilder>();
            var part = app.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            foreach (var ep in endpoints)
            {
                var assembly = controllerBuilder.Build(ep);
                part.ApplicationParts.Add(new AssemblyPart(assembly));
            }
        }
    }
}
