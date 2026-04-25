namespace DomainRelay.Mapping.Abstractions.Configuration;

/// <summary>
/// Provides access to mapping configuration validation.
/// </summary>
/// <remarks>
/// This service can be resolved from dependency injection to validate the mapping configuration
/// at application startup or in tests.
/// </remarks>
/// <example>
/// <code>
/// var provider = serviceProvider.GetRequiredService&lt;IMapperConfigurationProvider&gt;();
/// provider.AssertConfigurationIsValid();
/// </code>
/// </example>
public interface IMapperConfigurationProvider
{
    /// <summary>
    /// Validates all configured mappings.
    /// </summary>
    /// <exception cref="DomainRelay.Mapping.Abstractions.Exceptions.MappingValidationException">
    /// Thrown when one or more configured mappings are invalid.
    /// </exception>
    void AssertConfigurationIsValid();
}