namespace DomainRelay.Mapping.Abstractions.Configuration;

public interface IMappingOperationOptions
{
    IDictionary<string, object?> Items { get; }
}