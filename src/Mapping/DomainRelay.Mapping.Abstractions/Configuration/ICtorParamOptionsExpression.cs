using System.Linq.Expressions;

namespace DomainRelay.Mapping.Abstractions.Configuration;

/// <summary>
/// Configures how a destination constructor parameter is mapped.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TParam">The constructor parameter type.</typeparam>
/// <remarks>
/// Use this interface from <c>ForCtorParam</c> when the destination type is immutable,
/// record-like, or does not expose a public parameterless constructor.
/// </remarks>
/// <example>
/// <code>
/// configuration.CreateMap&lt;Tenant, TenantDto&gt;()
///     .ForCtorParam("displayName", opt =&gt; opt.MapFrom(s =&gt; s.Name));
/// </code>
/// </example>
public interface ICtorParamOptionsExpression<TSource, TParam>
{
    /// <summary>
    /// Maps the constructor parameter from a source expression.
    /// </summary>
    /// <param name="sourceExpression">
    /// The source expression used to compute the constructor parameter value.
    /// </param>
    void MapFrom(Expression<Func<TSource, TParam>> sourceExpression);

    /// <summary>
    /// Provides a replacement value when the resolved constructor argument is <see langword="null"/>.
    /// </summary>
    /// <param name="value">The replacement value.</param>
    void NullSubstitute(TParam value);
}