namespace DomainRelay.Mapping.Abstractions.Resolvers;

/// <summary>
/// Resolves a destination member value from the source and destination objects.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
/// <typeparam name="TDestMember">The destination member type.</typeparam>
/// <remarks>
/// Use a value resolver when the destination member requires custom logic that cannot be expressed
/// as a simple <c>MapFrom</c> expression.
/// </remarks>
/// <example>
/// <code>
/// public sealed class DisplayNameResolver : IValueResolver&lt;User, UserDto, string&gt;
/// {
///     public string Resolve(User source, UserDto destination)
///         =&gt; source.FirstName + " " + source.LastName;
/// }
/// </code>
/// </example>
public interface IValueResolver<in TSource, in TDestination, out TDestMember>
{
    /// <summary>
    /// Resolves the destination member value.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="destination">The destination object currently being mapped.</param>
    /// <returns>The resolved destination member value.</returns>
    TDestMember Resolve(TSource source, TDestination destination);
}