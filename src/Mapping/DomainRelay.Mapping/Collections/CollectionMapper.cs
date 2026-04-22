using System.Collections;
using DomainRelay.Mapping.Internal;

namespace DomainRelay.Mapping.Collections;

internal sealed class CollectionMapper : ICollectionMapper
{
    public bool CanMap(Type sourceType, Type destinationType)
    {
        return TypeHelper.IsEnumerable(sourceType)
               && TypeHelper.IsEnumerable(destinationType);
    }

    public object? MapCollection(
        object sourceCollection,
        object? destinationCollection,
        Type sourceType,
        Type destinationType,
        Func<object, Type, Type, object?> nestedMap)
    {
        var sourceElementType = TypeHelper.TryGetEnumerableElementType(sourceType);
        var destinationElementType = TypeHelper.TryGetEnumerableElementType(destinationType);

        if (sourceElementType is null || destinationElementType is null)
        {
            return null;
        }

        var sourceEnumerable = (IEnumerable)sourceCollection;
        var mappedItems = new List<object?>();

        foreach (var item in sourceEnumerable)
        {
            if (item is null)
            {
                mappedItems.Add(null);
                continue;
            }

            var runtimeSourceType = item.GetType();

            if (destinationElementType.IsAssignableFrom(runtimeSourceType))
            {
                mappedItems.Add(item);
                continue;
            }

            mappedItems.Add(nestedMap(item, runtimeSourceType, destinationElementType));
        }

        if (destinationCollection is IList existingList && !destinationType.IsArray)
        {
            existingList.Clear();

            foreach (var mappedItem in mappedItems)
            {
                existingList.Add(mappedItem);
            }

            return destinationCollection;
        }

        if (destinationType.IsArray)
        {
            var array = Array.CreateInstance(destinationElementType, mappedItems.Count);
            for (var i = 0; i < mappedItems.Count; i++)
            {
                array.SetValue(mappedItems[i], i);
            }

            return array;
        }

        var listType = typeof(List<>).MakeGenericType(destinationElementType);
        var list = (IList)Activator.CreateInstance(listType)!;

        foreach (var mappedItem in mappedItems)
        {
            list.Add(mappedItem);
        }

        if (destinationType.IsAssignableFrom(listType))
        {
            return list;
        }

        if (destinationType.IsInterface)
        {
            return list;
        }

        var destinationInstance = Activator.CreateInstance(destinationType);
        if (destinationInstance is IList destinationList)
        {
            foreach (var mappedItem in mappedItems)
            {
                destinationList.Add(mappedItem);
            }

            return destinationInstance;
        }

        return list;
    }
}