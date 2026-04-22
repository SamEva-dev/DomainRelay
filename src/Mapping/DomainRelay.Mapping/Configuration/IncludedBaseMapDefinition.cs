namespace DomainRelay.Mapping.Configuration;

internal sealed record IncludedBaseMapDefinition(
    Type BaseSourceType,
    Type BaseDestinationType);