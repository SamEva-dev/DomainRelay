namespace DomainRelay.Mapping.Abstractions.Configuration;

/// <summary>
/// Non-generic base contract for a configured map expression.
/// </summary>
/// <remarks>
/// This interface is primarily used by infrastructure components that need to inspect
/// mappings without knowing their source and destination types at compile time.
/// </remarks>
public interface IMapExpressionBase
{
    /// <summary>
    /// Gets the source type configured by this map.
    /// </summary>
    Type SourceType { get; }

    /// <summary>
    /// Gets the destination type configured by this map.
    /// </summary>
    Type DestinationType { get; }
}