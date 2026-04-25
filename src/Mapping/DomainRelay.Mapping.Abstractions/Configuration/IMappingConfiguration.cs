namespace DomainRelay.Mapping.Abstractions.Configuration;

/// <summary>
/// Defines the mapping configuration used by DomainRelay.Mapping.
/// </summary>
/// <remarks>
/// Use <see cref="CreateMap{TSource, TDestination}"/> inside a
/// <c>MappingProfile</c> to configure mappings between source and destination types.
/// </remarks>
public interface IMappingConfiguration
{
    /// <summary>
    /// Creates or retrieves a map between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <returns>A fluent mapping expression for the specified type pair.</returns>
    IMapExpression<TSource, TDestination> CreateMap<TSource, TDestination>();

    /// <summary>
    /// Creates or retrieves a map between runtime source and destination types.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <returns>A non-generic mapping expression for the specified type pair.</returns>
    IMapExpressionBase CreateMap(Type sourceType, Type destinationType);

    /// <summary>
    /// Attempts to retrieve an existing mapping expression for the specified type pair.
    /// </summary>
    /// <param name="sourceType">The source type.</param>
    /// <param name="destinationType">The destination type.</param>
    /// <param name="mapExpression">The mapping expression when found.</param>
    /// <returns><see langword="true"/> when a map exists; otherwise, <see langword="false"/>.</returns>
    bool TryGetMap(Type sourceType, Type destinationType, out object? mapExpression);

    /// <summary>
    /// Validates all configured maps and throws when configuration is invalid.
    /// </summary>
    /// <exception cref="DomainRelay.Mapping.Abstractions.Exceptions.MappingValidationException">
    /// Thrown when one or more configured maps are invalid.
    /// </exception>
    void AssertConfigurationIsValid();
}