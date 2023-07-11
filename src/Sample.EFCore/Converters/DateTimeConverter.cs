using ODatalizer.EFCore.Converters;
using System;

namespace Sample.EFCore.Converters
{
    public class DateTimeConverter : ITypeConverter
    {
        public Type ModelType => typeof(DateTime);

        public bool CanConvertFrom(Type type)
        {
            return type == typeof(DateTimeOffset);
        }

        private static readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
        public object Convert(object value)
        {
            var datetimeoffset = (DateTimeOffset)value;
            return datetimeoffset.ToOffset(_timeZoneInfo.GetUtcOffset(datetimeoffset)).LocalDateTime;
        }

        public bool TryParse(string str, out object value)
        {
            value = null;

            if (string.IsNullOrEmpty(str))
            {
                return true;
            }

            if (DateTimeOffset.TryParse(str, out var datetimeoffset))
            {
                value = Convert(datetimeoffset);
                return true;
            }

            else if (DateTime.TryParse(str, out var utc))
            {
                value = Convert(new DateTimeOffset(utc, TimeSpan.Zero));
                return true;
            }

            return false;
        }
    }
}
