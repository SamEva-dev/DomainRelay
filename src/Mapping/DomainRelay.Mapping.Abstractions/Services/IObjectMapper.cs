using DomainRelay.Mapping.Abstractions.Configuration;

namespace DomainRelay.Mapping.Abstractions.Services;

/// <summary>
/// Maps objects from a source type to a destination type.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IObjectMapper"/> is the main runtime mapping service of DomainRelay.Mapping.
/// It supports simple objects, nested objects, collections, dictionaries, enums, constructor-based
/// destination types, existing destination instances and mapping options.
/// </para>
/// <para>
/// For queryable database projections, prefer the projection services from
/// <c>DomainRelay.Mapping.Expressions</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var dto = mapper.Map&lt;TenantDto&gt;(tenant);
/// var dto2 = mapper.Map&lt;Tenant, TenantDto&gt;(tenant);
/// </code>
/// </example>
public interface IObjectMapper
{
    /// <summary>
    /// Maps the specified source object to <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TDestination">The destination type to create.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>A mapped destination instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> is <see langword="null"/>.</exception>
    TDestination Map<TDestination>(object source);

    /// <summary>
    /// Maps the specified source object to <typeparamref name="TDestination"/> using per-operation options.
    /// </summary>
    /// <typeparam name="TDestination">The destination type to create.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <param name="options">A callback used to configure runtime mapping options.</param>
    /// <returns>A mapped destination instance.</returns>
    TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> options);

    /// <summary>
    /// Maps a source object to a destination object using compile-time source and destination types.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type to create.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <returns>A mapped destination instance.</returns>
    /// <example>
    /// <code>
    /// TenantDto dto = mapper.Map&lt;Tenant, TenantDto&gt;(tenant);
    /// </code>
    /// </example>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Maps a source object to a destination object using per-operation options.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type to create.</typeparam>
    /// <param name="source">The source object to map.</param>
    /// <param name="options">A callback used to configure runtime mapping options.</param>
    /// <returns>A mapped destination instance.</returns>
    TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions> options);

    /// <summary>
    /// Maps a source object into an existing destination instance.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <param name="destination">The existing destination instance to update.</param>
    /// <returns>The updated destination instance.</returns>
    /// <remarks>
    /// This overload is useful for patch-like scenarios where the destination object already exists.
    /// </remarks>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    /// <summary>
    /// Maps a source object into an existing destination instance using per-operation options.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDestination">The destination type.</typeparam>
    /// <param name="source">The source object to map from.</param>
    /// <param name="destination">The existing destination instance to update.</param>
    /// <param name="options">A callback used to configure runtime mapping options.</param>
    /// <returns>The updated destination instance.</returns>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions> options);

    /// <summary>
    /// Maps an object using runtime source and destination types.
    /// </summary>
    /// <param name="source">The source object to map. May be <see langword="null"/>.</param>
    /// <param name="sourceType">The runtime source type.</param>
    /// <param name="destinationType">The runtime destination type.</param>
    /// <returns>The mapped destination object, or <see langword="null"/> when the source is <see langword="null"/>.</returns>
    object? Map(object? source, Type sourceType, Type destinationType);

    /// <summary>
    /// Maps an object using runtime source and destination types with per-operation options.
    /// </summary>
    /// <param name="source">The source object to map. May be <see langword="null"/>.</param>
    /// <param name="sourceType">The runtime source type.</param>
    /// <param name="destinationType">The runtime destination type.</param>
    /// <param name="options">A callback used to configure runtime mapping options.</param>
    /// <returns>The mapped destination object, or <see langword="null"/> when the source is <see langword="null"/>.</returns>
    object? Map(object? source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> options);

    /// <summary>
    /// Maps an object into an existing destination instance using runtime source and destination types.
    /// </summary>
    /// <param name="source">The source object to map. May be <see langword="null"/>.</param>
    /// <param name="destination">The existing destination instance to update.</param>
    /// <param name="sourceType">The runtime source type.</param>
    /// <param name="destinationType">The runtime destination type.</param>
    /// <returns>The updated destination object.</returns>
    object? Map(object? source, object destination, Type sourceType, Type destinationType);

    /// <summary>
    /// Maps an object into an existing destination instance using runtime types and per-operation options.
    /// </summary>
    /// <param name="source">The source object to map. May be <see langword="null"/>.</param>
    /// <param name="destination">The existing destination instance to update.</param>
    /// <param name="sourceType">The runtime source type.</param>
    /// <param name="destinationType">The runtime destination type.</param>
    /// <param name="options">A callback used to configure runtime mapping options.</param>
    /// <returns>The updated destination object.</returns>
    object? Map(object? source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> options);
}