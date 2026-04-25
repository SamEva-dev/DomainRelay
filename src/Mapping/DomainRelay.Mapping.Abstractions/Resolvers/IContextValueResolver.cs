using DomainRelay.Mapping.Abstractions.Services;

namespace DomainRelay.Mapping.Abstractions.Resolvers;

/// <summary>
/// Resolves a destination member value using the current mapping context.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
/// <typeparam name="TDestMember">The destination member type.</typeparam>
/// <remarks>
/// Context-aware resolvers can access dependency injection services and runtime values
/// passed through mapping operation options.
/// </remarks>
/// <example>
/// <code>
/// public sealed class LocalizedNameResolver : IContextValueResolver&lt;Product, ProductDto, string&gt;
/// {
///     public string Resolve(Product source, ProductDto destination, IMappingContext context)
///     {
///         var culture = context.Items.TryGetValue("culture", out var value)
///             ? value as string
///             : "en-US";
///
///         return source.GetName(culture);
///     }
/// }
/// </code>
/// </example>
public interface IContextValueResolver<in TSource, in TDestination, out TDestMember>
{
    /// <summary>
    /// Resolves the destination member value.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="destination">The destination object currently being mapped.</param>
    /// <param name="context">The current mapping context.</param>
    /// <returns>The resolved destination member value.</returns>
    TDestMember Resolve(TSource source, TDestination destination, IMappingContext context);
}