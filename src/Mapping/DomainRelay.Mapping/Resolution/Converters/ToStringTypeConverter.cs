using DomainRelay.Mapping.Abstractions.Converters;

namespace DomainRelay.Mapping.Resolution.Converters;

internal sealed class ToStringTypeConverter : ITypeConverter
{
    public bool CanConvert(Type sourceType, Type destinationType)
    {
        return destinationType == typeof(string);
    }

    public object? Convert(object? source, Type sourceType, Type destinationType)
    {
        return source?.ToString();
    }
}