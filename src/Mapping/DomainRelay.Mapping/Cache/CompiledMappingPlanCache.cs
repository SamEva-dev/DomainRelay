using System.Collections.Concurrent;
using DomainRelay.Mapping.Abstractions.Models;
using DomainRelay.Mapping.Planning;

namespace DomainRelay.Mapping.Cache;

internal sealed class CompiledMappingPlanCache
{
    private readonly ConcurrentDictionary<TypePair, CompiledMappingPlan> _cache = new();

    public bool TryGet(Type sourceType, Type destinationType, out CompiledMappingPlan? plan)
    {
        var found = _cache.TryGetValue(new TypePair(sourceType, destinationType), out var cached);
        plan = cached;
        return found;
    }

    public CompiledMappingPlan GetOrAdd(
        Type sourceType,
        Type destinationType,
        Func<CompiledMappingPlan> factory)
    {
        return _cache.GetOrAdd(new TypePair(sourceType, destinationType), _ => factory());
    }
}