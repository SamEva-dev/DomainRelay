namespace DomainRelay.Mapping.SourceGen.Models;

internal sealed class GeneratedMemberAssignmentModel
{
    public string DestinationMemberName { get; init; } = string.Empty;
    public string SourceAccessCode { get; init; } = string.Empty;
    public bool Ignored { get; init; }
    public string? NullSubstituteCode { get; init; }
    public bool RequiresNestedMapping { get; init; }
}