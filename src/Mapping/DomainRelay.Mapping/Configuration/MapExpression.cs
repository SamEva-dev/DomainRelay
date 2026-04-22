using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Internal;

namespace DomainRelay.Mapping.Configuration;

internal sealed class MapExpression<TSource, TDestination> : IMapExpression<TSource, TDestination>, IMapExpressionBase
{
    private readonly Dictionary<string, MemberMapDefinition> _memberMaps = new();
    private readonly Dictionary<string, CtorParamMapDefinition> _ctorParamMaps = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IncludedDerivedMapDefinition> _includedDerivedMaps = new();
    private readonly List<IncludedBaseMapDefinition> _includedBaseMaps = new();

    public Type SourceType => typeof(TSource);
    public Type DestinationType => typeof(TDestination);

    public Func<TSource, TDestination>? ConstructionFactory { get; private set; }

    public List<Action<TSource, TDestination>> BeforeMapActions { get; } = new();
    public List<Action<TSource, TDestination>> AfterMapActions { get; } = new();

    public IReadOnlyDictionary<string, MemberMapDefinition> MemberMaps => _memberMaps;
    public IReadOnlyDictionary<string, CtorParamMapDefinition> CtorParamMaps => _ctorParamMaps;
    public IReadOnlyList<IncludedDerivedMapDefinition> IncludedDerivedMaps => _includedDerivedMaps;
    public IReadOnlyList<IncludedBaseMapDefinition> IncludedBaseMaps => _includedBaseMaps;

    public IMapExpression<TSource, TDestination> ForMember<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember,
        Action<IMemberOptionsExpression<TSource, TDestination, TMember>> options)
    {
        var memberName = ExpressionHelper.GetMemberName(destinationMember);

        var memberOptions = new MemberOptionsExpression<TSource, TDestination, TMember>(memberName);
        options(memberOptions);

        _memberMaps[memberName] = memberOptions.Build();
        return this;
    }

    public IMapExpression<TSource, TDestination> ForCtorParam<TParam>(
        string parameterName,
        Action<ICtorParamOptionsExpression<TSource, TParam>> options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        var ctorOptions = new CtorParamOptionsExpression<TSource, TParam>(parameterName);
        options(ctorOptions);

        _ctorParamMaps[parameterName] = ctorOptions.Build();
        return this;
    }

    public IMapExpression<TSource, TDestination> Ignore<TMember>(
        Expression<Func<TDestination, TMember>> destinationMember)
    {
        var memberName = ExpressionHelper.GetMemberName(destinationMember);
        _memberMaps[memberName] = MemberMapDefinition.CreateIgnored(memberName);
        return this;
    }

    public IMapExpression<TSource, TDestination> ConstructUsing(Func<TSource, TDestination> factory)
    {
        ConstructionFactory = factory;
        return this;
    }

    public IMapExpression<TSource, TDestination> BeforeMap(Action<TSource, TDestination> action)
    {
        BeforeMapActions.Add(action);
        return this;
    }

    public IMapExpression<TSource, TDestination> AfterMap(Action<TSource, TDestination> action)
    {
        AfterMapActions.Add(action);
        return this;
    }

    public IMapExpression<TSource, TDestination> Include<TDerivedSource, TDerivedDestination>()
        where TDerivedSource : TSource
        where TDerivedDestination : TDestination
    {
        _includedDerivedMaps.Add(
            new IncludedDerivedMapDefinition(typeof(TDerivedSource), typeof(TDerivedDestination)));

        return this;
    }

    public IMapExpression<TSource, TDestination> IncludeBase<TBaseSource, TBaseDestination>()
        where TBaseSource : class
        where TBaseDestination : class
    {
        _includedBaseMaps.Add(
            new IncludedBaseMapDefinition(typeof(TBaseSource), typeof(TBaseDestination)));

        return this;
    }

    public IMapExpression<TDestination, TSource> ReverseMap()
    {
        throw new NotSupportedException("ReverseMap is handled by MapExpressionWrapper.");
    }
}