
namespace DomainRelay.Mapping.Configuration;

internal sealed record IncludedDerivedMapDefinition(
    Type DerivedSourceType,
    Type DerivedDestinationType);