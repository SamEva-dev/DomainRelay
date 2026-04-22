namespace DomainRelay.Mapping.Abstractions.Exceptions;

public sealed class MappingConfigurationException : MappingException
{
    public MappingConfigurationException(string message)
        : base(message)
    {
    }
}