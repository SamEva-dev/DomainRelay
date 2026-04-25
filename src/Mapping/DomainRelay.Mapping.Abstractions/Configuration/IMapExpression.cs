using System.Linq.Expressions;

namespace DomainRelay.Mapping.Abstractions.Configuration;

/// <summary>
/// Configures a mapping between <typeparamref name="TSource"/> and <typeparamref name="TDestination"/>.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
public interface IMapExpression<TSource, TDestination> : IMapExpressionBase
{
    /// <summary>
    /// Configures a destination member.
    /// </summary>
    /// <typeparam name="TMember">The destination member type.</typeparam>
    /// <param name="destinationMember">An expression selecting the destination member.</param>
    /// <param name="options">A callback used to configure the member mapping.</param>
    /// <returns>The current mapping expression.</returns>
    /// <example>
    /// <code>
    /// configuration.CreateMap&lt;Tenant, TenantDto&gt;()
    ///     .ForMember(d =&gt; d.DisplayName, opt =&gt; opt.MapFrom(s =&gt; s.Name));
    /// </code>
    /// </example>
    IMapExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberOptionsExpression<TSource, TDestination, TMember>> options);

    /// <summary>
    /// Configures a constructor parameter on the destination type.
    /// </summary>
    /// <typeparam name="TParam">The constructor parameter type.</typeparam>
    /// <param name="parameterName">The constructor parameter name.</param>
    /// <param name="options">A callback used to configure the constructor parameter mapping.</param>
    /// <returns>The current mapping expression.</returns>
    IMapExpression<TSource, TDestination> ForCtorParam<TParam>(
        string parameterName,
        Action<ICtorParamOptionsExpression<TSource, TParam>> options);

    /// <summary>
    /// Ignores a destination member.
    /// </summary>
    /// <typeparam name="TMember">The destination member type.</typeparam>
    /// <param name="destinationMember">An expression selecting the destination member to ignore.</param>
    /// <returns>The current mapping expression.</returns>
    IMapExpression<TSource, TDestination> Ignore<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember);

    /// <summary>
    /// Configures a factory used to create destination instances.
    /// </summary>
    /// <param name="factory">The factory used to create the destination object from the source object.</param>
    /// <returns>The current mapping expression.</returns>
    /// <remarks>
    /// This is useful for record types, immutable DTOs or custom construction logic.
    /// </remarks>
    IMapExpression<TSource, TDestination> ConstructUsing(
        Func<TSource, TDestination> factory);

    /// <summary>
    /// Registers an action executed before the member mapping starts.
    /// </summary>
    /// <param name="action">The action to execute before mapping.</param>
    /// <returns>The current mapping expression.</returns>
    IMapExpression<TSource, TDestination> BeforeMap(
        Action<TSource, TDestination> action);

    /// <summary>
    /// Registers an action executed after member mapping completes.
    /// </summary>
    /// <param name="action">The action to execute after mapping.</param>
    /// <returns>The current mapping expression.</returns>
    IMapExpression<TSource, TDestination> AfterMap(
        Action<TSource, TDestination> action);

    /// <summary>
    /// Includes a derived source/destination mapping for inheritance scenarios.
    /// </summary>
    /// <typeparam name="TDerivedSource">The derived source type.</typeparam>
    /// <typeparam name="TDerivedDestination">The derived destination type.</typeparam>
    /// <returns>The current mapping expression.</returns>
    IMapExpression<TSource, TDestination> Include<TDerivedSource, TDerivedDestination>()
        where TDerivedSource : TSource
        where TDerivedDestination : TDestination;

    /// <summary>
    /// Includes mapping rules from a base mapping.
    /// </summary>
    /// <typeparam name="TBaseSource">The base source type.</typeparam>
    /// <typeparam name="TBaseDestination">The base destination type.</typeparam>
    /// <returns>The current mapping expression.</returns>
    IMapExpression<TSource, TDestination> IncludeBase<TBaseSource, TBaseDestination>()
        where TBaseSource : class
        where TBaseDestination : class;

    /// <summary>
    /// Creates the reverse mapping from <typeparamref name="TDestination"/> to <typeparamref name="TSource"/>.
    /// </summary>
    /// <returns>A mapping expression for the reverse map.</returns>
    IMapExpression<TDestination, TSource> ReverseMap();
}