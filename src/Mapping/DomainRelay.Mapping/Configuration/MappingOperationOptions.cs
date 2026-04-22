using DomainRelay.Mapping.Abstractions.Configuration;

namespace DomainRelay.Mapping.Configuration;

internal sealed class MappingOperationOptions : IMappingOperationOptions
{
    public IDictionary<string, object?> Items { get; } =
        new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
}