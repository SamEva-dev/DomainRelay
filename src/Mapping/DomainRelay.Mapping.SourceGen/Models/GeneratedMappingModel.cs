namespace DomainRelay.Mapping.SourceGen.Models;

internal sealed class GeneratedMappingModel
{
    public string Namespace { get; init; } = "DomainRelay.Mapping.Generated";
    public string SourceTypeDisplayName { get; init; } = string.Empty;
    public string DestinationTypeDisplayName { get; init; } = string.Empty;
    public string MappingMethodName { get; init; } = string.Empty;
    public string BoxedMethodName { get; init; } = string.Empty;
    public IReadOnlyList<GeneratedMemberAssignmentModel> Members { get; init; } = Array.Empty<GeneratedMemberAssignmentModel>();
}