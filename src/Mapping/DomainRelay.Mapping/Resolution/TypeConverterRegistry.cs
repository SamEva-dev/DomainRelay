using DomainRelay.Mapping.Abstractions.Converters;

namespace DomainRelay.Mapping.Resolution;

public sealed class TypeConverterRegistry
{
    private readonly List<ITypeConverter> _converters = new();

    public TypeConverterRegistry(IEnumerable<ITypeConverter>? converters = null)
    {
        if (converters is not null)
        {
            _converters.AddRange(converters);
        }
    }

    public void Register(ITypeConverter converter)
    {
        _converters.Add(converter);
    }

    public bool TryConvert(object? source, Type sourceType, Type destinationType, out object? result)
    {
        foreach (var converter in _converters)
        {
            if (!converter.CanConvert(sourceType, destinationType))
            {
                continue;
            }

            result = converter.Convert(source, sourceType, destinationType);
            return true;
        }

        result = null;
        return false;
    }
}