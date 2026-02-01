namespace DomainRelay.Exceptions;

public sealed class DomainRelayException : Exception
{
    public DomainRelayException(string message, Exception inner) : base(message, inner) { }
}
