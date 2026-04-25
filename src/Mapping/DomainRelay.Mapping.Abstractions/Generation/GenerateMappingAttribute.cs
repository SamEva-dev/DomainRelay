namespace DomainRelay.Mapping.Abstractions.Generation;

/// <summary>
/// Requests source generation for a mapping pair.
/// </summary>
/// <remarks>
/// Apply this attribute to a class to instruct DomainRelay.Mapping.SourceGen to generate
/// optimized mapping code for the specified source and destination types.
/// </remarks>
/// <example>
/// <code>
/// [GenerateMapping(typeof(Tenant), typeof(TenantDto))]
/// public partial class MappingGenerationMarker
/// {
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class GenerateMappingAttribute : Attribute
{
    /// <summary>
    /// Gets the source type to generate a mapper for.
    /// </summary>
    public Type SourceType { get; }

    /// <summary>
    /// Gets the destination type to generate a mapper for.
    /// </summary>
    public Type DestinationType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateMappingAttribute"/> class.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="destinationType">The destination type.</param>
    public GenerateMappingAttribute(Type sourceType, Type destinationType)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
    }
}