using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Threading.Tasks;

namespace Sample.EFCore.Binders
{
    public class DateTimeBinder : IModelBinder
    {
        private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

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

            if (DateTimeOffset.TryParse(value, out var datetimeoffset))
            {
                var datetime = datetimeoffset.ToOffset(_timeZoneInfo.GetUtcOffset(datetimeoffset)).LocalDateTime;
                bindingContext.Result = ModelBindingResult.Success(datetime);
                return Task.CompletedTask;
            }

            else if (DateTime.TryParse(value, out var utc))
            {
                var datetime = new DateTimeOffset(utc, TimeSpan.Zero).ToOffset(_timeZoneInfo.GetUtcOffset(datetimeoffset)).LocalDateTime;
                bindingContext.Result = ModelBindingResult.Success(datetime);
                return Task.CompletedTask;
            }

            bindingContext.ModelState.TryAddModelError(modelName, "Invalid DateTime format.");

            return Task.CompletedTask;
        }
    }
}
