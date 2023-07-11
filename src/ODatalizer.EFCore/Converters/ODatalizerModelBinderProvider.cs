using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ODatalizer.EFCore.Converters
{
    public class ODatalizerModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var typeConverters = context.Services.GetService<IEnumerable<ITypeConverter>>();

            if (typeConverters != null)
            {
                if (typeConverters.Any(o => context.Metadata.ModelType == o.ModelType))
                {
                    return new BinderTypeModelBinder(typeof(ODatalizerModelBinder));
                }
            }

            return null;
        }
    }
}
