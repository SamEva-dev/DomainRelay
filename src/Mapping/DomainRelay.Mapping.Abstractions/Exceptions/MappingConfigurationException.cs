namespace DomainRelay.Mapping.Abstractions.Exceptions;

/// <summary>
/// Represents an invalid mapping configuration.
/// </summary>
public sealed class MappingConfigurationException : MappingException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingConfigurationException"/> class.
    /// </summary>
    /// <param name="message">The configuration error message.</param>
    public MappingConfigurationException(string message)
        : base(message)
    {
    }
}