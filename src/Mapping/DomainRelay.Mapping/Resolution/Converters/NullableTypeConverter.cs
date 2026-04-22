using DomainRelay.Mapping.Abstractions.Converters;

namespace DomainRelay.Mapping.Resolution.Converters;

internal sealed class NullableTypeConverter : ITypeConverter
{
    public bool CanConvert(Type sourceType, Type destinationType)
    {
        var underlying = Nullable.GetUnderlyingType(destinationType);
        return underlying is not null && underlying == sourceType;
    }

    public object? Convert(object? source, Type sourceType, Type destinationType)
    {
        return source;
    }
}