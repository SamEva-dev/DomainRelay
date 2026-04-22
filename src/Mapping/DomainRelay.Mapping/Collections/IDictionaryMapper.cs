namespace DomainRelay.Mapping.Collections;

internal interface IDictionaryMapper
{
    bool CanMap(Type sourceType, Type destinationType);

    object? MapDictionary(
        object source,
        object? destination,
        Type sourceType,
        Type destinationType,
        Func<object, Type, Type, object?> nestedMap);
}