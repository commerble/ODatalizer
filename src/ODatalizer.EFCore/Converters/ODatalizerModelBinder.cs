using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODatalizer.EFCore.Converters
{
    public class ODatalizerModelBinder : IModelBinder
    {
        private readonly IEnumerable<ITypeConverter> _typeConverters;

        public ODatalizerModelBinder(IEnumerable<ITypeConverter> typeConverters)
        {
            _typeConverters = typeConverters;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }
            var modelName = bindingContext.ModelName;

            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            var converter = _typeConverters.FirstOrDefault(o => o.ModelType == bindingContext.ModelType);

            if (converter == null)
            {
                return Task.CompletedTask;
            }

            if (converter.TryParse(value, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(modelName, "Invalid DateTime format.");

            return Task.CompletedTask;
        }
    }
}
