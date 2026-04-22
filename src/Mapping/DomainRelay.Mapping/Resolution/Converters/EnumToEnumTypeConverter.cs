using DomainRelay.Mapping.Abstractions.Converters;

namespace DomainRelay.Mapping.Resolution.Converters;

internal sealed class EnumToEnumTypeConverter : ITypeConverter
{
    public bool CanConvert(Type sourceType, Type destinationType)
    {
        return sourceType.IsEnum && destinationType.IsEnum;
    }

    public object? Convert(object? source, Type sourceType, Type destinationType)
    {
        if (source is null)
        {
            return null;
        }

        var sourceName = Enum.GetName(sourceType, source);
        if (sourceName is not null &&
            Enum.GetNames(destinationType).Contains(sourceName, StringComparer.OrdinalIgnoreCase))
        {
            return Enum.Parse(destinationType, sourceName, ignoreCase: true);
        }

        var numeric = System.Convert.ChangeType(source, Enum.GetUnderlyingType(sourceType));
        return Enum.ToObject(destinationType, numeric!);
    }
}