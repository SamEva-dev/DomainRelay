namespace DomainRelay.Mapping.Abstractions.Converters;

public interface ITypeConverter
{
    bool CanConvert(Type sourceType, Type destinationType);

    object? Convert(object? source, Type sourceType, Type destinationType);
}