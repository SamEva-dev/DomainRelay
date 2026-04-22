namespace DomainRelay.Mapping.Diagnostics;

internal sealed class InMemoryMappingDiagnosticsCollector : IMappingDiagnosticsCollector
{
    private readonly List<MappingDiagnostic> _items = new();

    public void Add(MappingDiagnostic diagnostic)
    {
        _items.Add(diagnostic);
    }

    public IReadOnlyList<MappingDiagnostic> Items => _items;
}