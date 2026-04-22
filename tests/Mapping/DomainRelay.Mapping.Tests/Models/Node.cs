namespace DomainRelay.Mapping.Tests.Models;

public sealed class Node
{
    public string Name { get; set; } = string.Empty;
    public Node? Child { get; set; }
}