using System.Collections.Generic;

namespace ODatalizer.Extensions
{
    internal static class StringExtensions
    {
        public static string Join(this IEnumerable<string> values, string separator) => string.Join(separator, values);
    }
}
