namespace DomainRelay.Mapping.Abstractions.Models;

/// <summary>
/// Represents a source/destination type pair.
/// </summary>
/// <param name="SourceType">The source type.</param>
/// <param name="DestinationType">The destination type.</param>
/// <remarks>
/// This value is commonly used as a dictionary key for mapping plans, compiled delegates
/// and configuration lookups.
/// </remarks>
public readonly record struct TypePair(Type SourceType, Type DestinationType);