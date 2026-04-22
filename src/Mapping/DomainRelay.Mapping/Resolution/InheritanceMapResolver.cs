namespace DomainRelay.Mapping.Resolution;

using DomainRelay.Mapping.Configuration;

internal static class InheritanceMapResolver
{
    public static (Type SourceType, Type DestinationType)? TryResolveMoreSpecificMap(
        MappingConfiguration configuration,
        Type requestedSourceType,
        Type runtimeSourceType,
        Type requestedDestinationType)
    {
        if (configuration.TryGetMap(runtimeSourceType, requestedDestinationType, out _))
        {
            return (runtimeSourceType, requestedDestinationType);
        }

        var directIncluded = TryResolveIncludedDestination(
            configuration,
            requestedSourceType,
            runtimeSourceType,
            requestedDestinationType);

        if (directIncluded is not null)
        {
            return directIncluded;
        }

        foreach (var sourceCandidate in EnumerateSourceHierarchy(runtimeSourceType))
        {
            var included = TryResolveIncludedDestination(
                configuration,
                sourceCandidate,
                runtimeSourceType,
                requestedDestinationType);

            if (included is not null)
            {
                return included;
            }

            if (configuration.TryGetMap(sourceCandidate, requestedDestinationType, out _))
            {
                return (sourceCandidate, requestedDestinationType);
            }
        }

        var candidateDestinationTypes = requestedDestinationType.Assembly
            .GetTypes()
            .Where(t => requestedDestinationType.IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var sourceCandidate in EnumerateSourceHierarchy(runtimeSourceType))
        {
            foreach (var candidateDestinationType in candidateDestinationTypes)
            {
                if (configuration.TryGetMap(sourceCandidate, candidateDestinationType, out _))
                {
                    return (sourceCandidate, candidateDestinationType);
                }
            }
        }

        return null;
    }

    private static (Type SourceType, Type DestinationType)? TryResolveIncludedDestination(
        MappingConfiguration configuration,
        Type baseSourceType,
        Type runtimeSourceType,
        Type requestedDestinationType)
    {
        if (!configuration.TryGetMap(baseSourceType, requestedDestinationType, out var mapExpressionObject))
        {
            return null;
        }

        if (mapExpressionObject is null)
        {
            return null;
        }

        var includedMaps = GetIncludedDerivedMaps(mapExpressionObject);
        foreach (var includedMap in includedMaps)
        {
            if (!includedMap.DerivedSourceType.IsAssignableFrom(runtimeSourceType))
            {
                continue;
            }

            if (!requestedDestinationType.IsAssignableFrom(includedMap.DerivedDestinationType))
            {
                continue;
            }

            if (configuration.TryGetMap(includedMap.DerivedSourceType, includedMap.DerivedDestinationType, out _))
            {
                return (includedMap.DerivedSourceType, includedMap.DerivedDestinationType);
            }
        }

        return null;
    }

    private static IReadOnlyList<IncludedDerivedMapDefinition> GetIncludedDerivedMaps(object mapExpressionObject)
    {
        var property = mapExpressionObject
            .GetType()
            .GetProperty("IncludedDerivedMaps");

        if (property?.GetValue(mapExpressionObject) is IReadOnlyList<IncludedDerivedMapDefinition> typed)
        {
            return typed;
        }

        return Array.Empty<IncludedDerivedMapDefinition>();
    }

    private static IEnumerable<Type> EnumerateSourceHierarchy(Type runtimeSourceType)
    {
        var current = runtimeSourceType.BaseType;
        while (current is not null && current != typeof(object))
        {
            yield return current;
            current = current.BaseType;
        }
    }
}
