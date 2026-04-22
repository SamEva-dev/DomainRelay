namespace DomainRelay.Mapping.Abstractions.Services;

public interface IMappingContext
{
    IServiceProvider? ServiceProvider { get; }

    IReadOnlyDictionary<string, object?> Items { get; }

    bool TryGetVisited(object source, Type destinationType, out object? destination);

    void RegisterVisited(object source, Type destinationType, object destination);
}