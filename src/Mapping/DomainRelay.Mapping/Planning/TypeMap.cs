using DomainRelay.Mapping.Configuration;

namespace DomainRelay.Mapping.Planning;

internal sealed class TypeMap
{
    public Type SourceType { get; }
    public Type DestinationType { get; }
    public Func<object, object>? ConstructionFactory { get; }
    public IReadOnlyDictionary<string, CtorParamMapDefinition> CtorParamMaps { get; }
    public IReadOnlyList<MemberMap> MemberMaps { get; }
    public IReadOnlyList<Action<object, object>> BeforeMapActions { get; }
    public IReadOnlyList<Action<object, object>> AfterMapActions { get; }

    public TypeMap(
        Type sourceType,
        Type destinationType,
        Func<object, object>? constructionFactory,
        IReadOnlyDictionary<string, CtorParamMapDefinition> ctorParamMaps,
        IReadOnlyList<MemberMap> memberMaps,
        IReadOnlyList<Action<object, object>> beforeMapActions,
        IReadOnlyList<Action<object, object>> afterMapActions)
    {
        SourceType = sourceType;
        DestinationType = destinationType;
        ConstructionFactory = constructionFactory;
        CtorParamMaps = ctorParamMaps;
        MemberMaps = memberMaps;
        BeforeMapActions = beforeMapActions;
        AfterMapActions = afterMapActions;
    }
}
