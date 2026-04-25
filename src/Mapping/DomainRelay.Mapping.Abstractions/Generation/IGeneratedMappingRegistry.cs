namespace DomainRelay.Mapping.Abstractions.Generation;

/// <summary>
/// Provides access to source-generated mapping delegates.
/// </summary>
/// <remarks>
/// Runtime mapping services can use this registry to locate generated mappers before falling back
/// to reflection or compiled expression-based mapping.
/// </remarks>
public interface IGeneratedMappingRegistry
{
    /// <summary>
    /// Attempts to get a generated mapper for the specified source and destination types.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <param name="mapper">The generated mapper delegate when found.</param>
    /// <returns><see langword="true"/> when a generated mapper exists; otherwise, <see langword="false"/>.</returns>
    bool TryGetGeneratedMapper(Type sourceType, Type destinationType, out Func<object, object>? mapper);
}