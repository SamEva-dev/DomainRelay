using System.Collections.Concurrent;
using DomainRelay.Mapping.Abstractions.Models;
using DomainRelay.Mapping.Planning;

namespace DomainRelay.Mapping.Cache;

internal sealed class MappingPlanCache
{
    private readonly ConcurrentDictionary<TypePair, MappingPlan> _cache = new();

    public bool TryGet(Type sourceType, Type destinationType, out MappingPlan? plan)
    {
        var found = _cache.TryGetValue(new TypePair(sourceType, destinationType), out var cached);
        plan = cached;
        return found;
    }

    public MappingPlan GetOrAdd(Type sourceType, Type destinationType, Func<MappingPlan> factory)
    {
        return _cache.GetOrAdd(new TypePair(sourceType, destinationType), _ => factory());
    }
}