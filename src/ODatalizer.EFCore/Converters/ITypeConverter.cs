using System;

namespace ODatalizer.EFCore.Converters
{
    public interface ITypeConverter
    {
        Type ModelType { get; }
        bool CanConvertFrom(Type type);
        object Convert(object value);
        bool TryParse(string str, out object value);
    }
}
