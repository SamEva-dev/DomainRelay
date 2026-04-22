using DomainRelay.Mapping.Abstractions.Services;

namespace DomainRelay.Mapping.Engine;

internal sealed class MappingContext : IMappingContext
{
    private readonly Dictionary<(int SourceHash, Type DestinationType), object> _visited = new();

    public IServiceProvider? ServiceProvider { get; }

    public IReadOnlyDictionary<string, object?> Items { get; }

    public MappingContext(
        IServiceProvider? serviceProvider = null,
        IReadOnlyDictionary<string, object?>? items = null)
    {
        ServiceProvider = serviceProvider;
        Items = items ?? new Dictionary<string, object?>();
    }

    public bool TryGetVisited(object source, Type destinationType, out object? destination)
    {
        var key = (System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(source), destinationType);
        return _visited.TryGetValue(key, out destination);
    }

    public void RegisterVisited(object source, Type destinationType, object destination)
    {
        var key = (System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(source), destinationType);
        _visited[key] = destination;
    }
}