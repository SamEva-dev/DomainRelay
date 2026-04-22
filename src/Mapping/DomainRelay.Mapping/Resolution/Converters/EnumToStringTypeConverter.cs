using DomainRelay.Mapping.Abstractions.Converters;

namespace DomainRelay.Mapping.Resolution.Converters;

internal sealed class EnumToStringTypeConverter : ITypeConverter
{
    public bool CanConvert(Type sourceType, Type destinationType)
    {
        return sourceType.IsEnum && destinationType == typeof(string);
    }

    public object? Convert(object? source, Type sourceType, Type destinationType)
    {
        return source?.ToString();
    }
}