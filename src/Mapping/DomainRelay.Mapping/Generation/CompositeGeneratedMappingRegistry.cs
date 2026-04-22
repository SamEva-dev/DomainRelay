using DomainRelay.Mapping.Abstractions.Generation;

namespace DomainRelay.Mapping.Generation;

internal sealed class CompositeGeneratedMappingRegistry : IGeneratedMappingRegistry
{
    private readonly IReadOnlyList<IGeneratedMappingRegistry> _registries;

    public CompositeGeneratedMappingRegistry(IEnumerable<IGeneratedMappingRegistry> registries)
    {
        _registries = registries?.ToArray() ?? Array.Empty<IGeneratedMappingRegistry>();
    }

    public bool TryGetGeneratedMapper(Type sourceType, Type destinationType, out Func<object, object>? mapper)
    {
        foreach (var registry in _registries)
        {
            if (registry.TryGetGeneratedMapper(sourceType, destinationType, out mapper) && mapper is not null)
            {
                return true;
            }
        }

        mapper = null;
        return false;
    }
}