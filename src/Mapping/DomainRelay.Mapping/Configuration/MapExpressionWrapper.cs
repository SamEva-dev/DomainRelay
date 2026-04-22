using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Configuration;

namespace DomainRelay.Mapping.Configuration;

internal sealed class MapExpressionWrapper<TSource, TDestination> : IMapExpression<TSource, TDestination>
{
    private readonly MappingConfiguration _configuration;
    private readonly MapExpression<TSource, TDestination> _inner;

    public MapExpressionWrapper(
        MappingConfiguration configuration,
        MapExpression<TSource, TDestination> inner)
    {
        _configuration = configuration;
        _inner = inner;
    }

    public Type SourceType => typeof(TSource);
    public Type DestinationType => typeof(TDestination);

    public IMapExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberOptionsExpression<TSource, TDestination, TMember>> options)
    {
        _inner.ForMember(destinationMember, options);
        return this;
    }

    public IMapExpression<TSource, TDestination> ForCtorParam<TParam>(
        string parameterName,
        Action<ICtorParamOptionsExpression<TSource, TParam>> options)
    {
        _inner.ForCtorParam(parameterName, options);
        return this;
    }

    public IMapExpression<TSource, TDestination> Ignore<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember)
    {
        _inner.Ignore(destinationMember);
        return this;
    }

    public IMapExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> factory)
    {
        _inner.ConstructUsing(factory);
        return this;
    }

    public IMapExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> action)
    {
        _inner.BeforeMap(action);
        return this;
    }

    public IMapExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> action)
    {
        _inner.AfterMap(action);
        return this;
    }

    public IMapExpression<TSource, TDestination> Include<TDerivedSource, TDerivedDestination>()
        where TDerivedSource : TSource
        where TDerivedDestination : TDestination
    {
        _inner.Include<TDerivedSource, TDerivedDestination>();
        return this;
    }

    public IMapExpression<TSource, TDestination> IncludeBase<TBaseSource, TBaseDestination>()
        where TBaseSource : class
        where TBaseDestination : class
    {
        _inner.IncludeBase<TBaseSource, TBaseDestination>();
        return this;
    }

    public IMapExpression<TDestination, TSource> ReverseMap()
    {
        return _configuration.CreateReverseMap(_inner);
    }
}