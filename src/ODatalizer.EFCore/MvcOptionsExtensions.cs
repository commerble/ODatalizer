using Microsoft.AspNetCore.Mvc;
using ODatalizer.EFCore.Converters;

namespace ODatalizer.EFCore
{
    public static class MvcOptionsExtensions
    {
        public static void AddODatalizerOptions(this MvcOptions options)
        {
            options.ModelBinderProviders.Insert(0, new ODatalizerModelBinderProvider());
        }
    }
}
