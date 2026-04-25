namespace DomainRelay.Mapping.Abstractions.Exceptions;

/// <summary>
/// Base exception for DomainRelay.Mapping errors.
/// </summary>
public class MappingException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MappingException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public MappingException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public MappingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}