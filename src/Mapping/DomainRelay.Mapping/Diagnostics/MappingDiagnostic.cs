namespace DomainRelay.Mapping.Diagnostics;

public sealed class MappingDiagnostic
{
    public string Category { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Type? SourceType { get; init; }
    public Type? DestinationType { get; init; }
    public string? MemberName { get; init; }
}