namespace DomainRelay.Mapping.Abstractions.Configuration;

public sealed class MappingRuntimeOptions
{
    public bool EnableDiagnostics { get; set; }
    public bool EnableFastPathCompilation { get; set; } = true;
    public bool ThrowOnMappingFailure { get; set; } = true;
}