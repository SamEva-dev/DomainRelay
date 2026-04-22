namespace DomainRelay.Mapping.Abstractions.Configuration;

public interface IMapExpressionBase
{
    Type SourceType { get; }
    Type DestinationType { get; }
}