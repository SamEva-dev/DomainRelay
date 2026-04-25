namespace DomainRelay.Mapping.Abstractions.Configuration;

/// <summary>
/// Runtime options controlling DomainRelay.Mapping behavior.
/// </summary>
public sealed class MappingRuntimeOptions
{
    /// <summary>
    /// Gets or sets whether mapping diagnostics should be enabled.
    /// </summary>
    /// <remarks>
    /// Diagnostics may be useful during development or tests, but can add overhead
    /// depending on the runtime implementation.
    /// </remarks>
    public bool EnableDiagnostics { get; set; }

    /// <summary>
    /// Gets or sets whether compiled fast-path mapping should be enabled when available.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="true"/>.
    /// </remarks>
    public bool EnableFastPathCompilation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether mapping failures should throw exceptions.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="true"/>. Production applications should usually keep this enabled
    /// to avoid silently returning incomplete destination objects.
    /// </remarks>
    public bool ThrowOnMappingFailure { get; set; } = true;
}