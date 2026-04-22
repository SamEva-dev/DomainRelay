namespace DomainRelay.Mapping.Tests.Models;

public sealed class NodeDto
{
    public string Name { get; set; } = string.Empty;
    public NodeDto? Child { get; set; }
}