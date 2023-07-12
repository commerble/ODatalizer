using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using ODatalizer.EFCore.Builders;
using Microsoft.AspNetCore.OData;

namespace ODatalizer.EFCore
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseODatalizer(this IApplicationBuilder app)
        {
            app.UseODataBatching();
        }
    }
}
