using DomainRelay.Mapping.Engine;

namespace DomainRelay.Mapping.Planning;

internal delegate object? CompiledMappingDelegate(
    object source,
    object? destination,
    MappingContext context);