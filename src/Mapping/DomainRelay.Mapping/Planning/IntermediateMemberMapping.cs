namespace DomainRelay.Mapping.Planning;

internal sealed class IntermediateMemberMapping
{
    public string DestinationMemberName { get; init; } = string.Empty;
    public string? SourceMemberPath { get; init; }
    public bool IsIgnored { get; init; }
    public bool HasCondition { get; init; }
    public bool HasConverter { get; init; }
}