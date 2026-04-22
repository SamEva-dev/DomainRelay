namespace DomainRelay.Mapping.Collections;

internal interface ICollectionMapper
{
    bool CanMap(Type sourceType, Type destinationType);

    object? MapCollection(
        object sourceCollection,
        object? destinationCollection,
        Type sourceType,
        Type destinationType,
        Func<object, Type, Type, object?> nestedMap);
}