using DomainRelay.Mapping.Abstractions.Converters;

namespace DomainRelay.Mapping.Resolution.Converters;

internal sealed class NumberToEnumTypeConverter : ITypeConverter
{
    public bool CanConvert(Type sourceType, Type destinationType)
    {
        var actualSource = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
        return destinationType.IsEnum
               && (actualSource.IsPrimitive || actualSource == typeof(decimal));
    }

    public object? Convert(object? source, Type sourceType, Type destinationType)
    {
        if (source is null)
        {
            return null;
        }

        return Enum.ToObject(destinationType, source);
    }
}