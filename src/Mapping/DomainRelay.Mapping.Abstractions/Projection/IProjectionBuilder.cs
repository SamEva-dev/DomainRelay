using System.Linq.Expressions;

namespace DomainRelay.Mapping.Abstractions.Projection;

/// <summary>
/// Builds LINQ projection expressions from configured mappings.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IProjectionBuilder"/> is used by <c>DomainRelay.Mapping.Expressions</c>
/// to project <see cref="IQueryable{T}"/> sources directly to DTOs.
/// </para>
/// <para>
/// Only expression-translatable mappings can be projected. Runtime-only logic such as
/// arbitrary methods, JSON deserialization, service-based resolvers or non-translatable converters
/// should remain in in-memory mapping.
/// </para>
/// </remarks>
public interface IProjectionBuilder
{
    /// <summary>
    /// Builds a projection expression from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">The source entity or model type.</typeparam>
    /// <typeparam name="TDestination">The destination DTO or read model type.</typeparam>
    /// <returns>A LINQ projection expression.</returns>
    Expression<Func<TSource, TDestination>> BuildProjection<TSource, TDestination>();

    /// <summary>
    /// Builds a projection expression using runtime source and destination types.
    /// </summary>
    /// <param name="sourceType">The source entity or model type.</param>
    /// <param name="destinationType">The destination DTO or read model type.</param>
    /// <returns>A LINQ projection expression.</returns>
    LambdaExpression BuildProjection(Type sourceType, Type destinationType);
}