using DomainRelay.Mapping.Abstractions.Converters;

namespace DomainRelay.Mapping.Resolution.Converters;

internal sealed class EnumByNameTypeConverter : ITypeConverter
{
    public bool CanConvert(Type sourceType, Type destinationType)
    {
        return sourceType == typeof(string) && destinationType.IsEnum;
    }

    public object? Convert(object? source, Type sourceType, Type destinationType)
    {
        if (source is not string text || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return Enum.Parse(destinationType, text, ignoreCase: true);
    }
}