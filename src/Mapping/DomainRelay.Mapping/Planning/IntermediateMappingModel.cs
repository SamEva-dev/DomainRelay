namespace DomainRelay.Mapping.Planning;

internal sealed class IntermediateMappingModel
{
    public Type SourceType { get; init; } = null!;
    public Type DestinationType { get; init; } = null!;
    public IReadOnlyList<IntermediateMemberMapping> Members { get; init; } = Array.Empty<IntermediateMemberMapping>();
}