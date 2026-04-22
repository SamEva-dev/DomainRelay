namespace DomainRelay.Mapping.Diagnostics;

public interface IMappingDiagnosticsCollector
{
    void Add(MappingDiagnostic diagnostic);
}