namespace DomainRelay.Exceptions;

/// <summary>
/// Represents an error raised by the DomainRelay mediator runtime.
/// </summary>
/// <remarks>
/// This exception is used when <c>DomainRelayOptions.WrapExceptions</c> is enabled.
/// It wraps the original exception thrown by a request handler, notification handler,
/// pipeline behavior or publish strategy.
/// </remarks>
public sealed class DomainRelayException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainRelayException"/> class.
    /// </summary>
    /// <param name="message">The mediator error message.</param>
    /// <param name="inner">The original exception.</param>
    public DomainRelayException(string message, Exception inner)
        : base(message, inner)
    {
    }
}