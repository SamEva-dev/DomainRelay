using DomainRelay.Mapping.Abstractions.Configuration;

namespace DomainRelay.Mapping.Configuration;

internal sealed class OpenGenericMapExpression : IMapExpressionBase
{
    public Type SourceType { get; }
    public Type DestinationType { get; }

    public OpenGenericMapExpression(Type sourceType, Type destinationType)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }
}