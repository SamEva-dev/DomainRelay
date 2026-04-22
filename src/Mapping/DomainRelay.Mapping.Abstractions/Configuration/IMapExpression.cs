using System.Linq.Expressions;

namespace DomainRelay.Mapping.Abstractions.Configuration;

public interface IMapExpression<TSource, TDestination> : IMapExpressionBase
{
    IMapExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberOptionsExpression<TSource, TDestination, TMember>> options);

    IMapExpression<TSource, TDestination> ForCtorParam<TParam>(
        string parameterName,
        Action<ICtorParamOptionsExpression<TSource, TParam>> options);

    IMapExpression<TSource, TDestination> Ignore<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember);

    IMapExpression<TSource, TDestination> ConstructUsing(
        Func<TSource, TDestination> factory);

    IMapExpression<TSource, TDestination> BeforeMap(
        Action<TSource, TDestination> action);

    IMapExpression<TSource, TDestination> AfterMap(
        Action<TSource, TDestination> action);

    IMapExpression<TSource, TDestination> Include<TDerivedSource, TDerivedDestination>()
        where TDerivedSource : TSource
        where TDerivedDestination : TDestination;

    IMapExpression<TSource, TDestination> IncludeBase<TBaseSource, TBaseDestination>()
        where TBaseSource : class
        where TBaseDestination : class;

    IMapExpression<TDestination, TSource> ReverseMap();
}