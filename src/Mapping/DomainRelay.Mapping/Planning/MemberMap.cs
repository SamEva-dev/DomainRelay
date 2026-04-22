using System.Reflection;
using DomainRelay.Mapping.Abstractions.Services;

namespace DomainRelay.Mapping.Planning;

internal sealed class MemberMap
{
    public string DestinationMemberName { get; }
    public PropertyInfo DestinationProperty { get; }
    public Func<object, object?>? ValueResolver { get; }
    public Func<object, object, IMappingContext, object?>? ContextValueResolver { get; }
    public Type? ResolverType { get; }
    public bool UsesContextResolverType { get; }
    public Func<object, bool>? PreCondition { get; }
    public Func<object, object, bool>? Condition { get; }
    public object? NullSubstitute { get; }
    public bool Ignored { get; }
    public bool IsExplicit { get; }

    public MemberMap(
        string destinationMemberName,
        PropertyInfo destinationProperty,
        Func<object, object?>? valueResolver,
        Func<object, object, IMappingContext, object?>? contextValueResolver,
        Type? resolverType,
        bool usesContextResolverType,
        Func<object, bool>? preCondition,
        Func<object, object, bool>? condition,
        object? nullSubstitute,
        bool ignored,
        bool isExplicit)
    {
        DestinationMemberName = destinationMemberName;
        DestinationProperty = destinationProperty;
        ValueResolver = valueResolver;
        ContextValueResolver = contextValueResolver;
        ResolverType = resolverType;
        UsesContextResolverType = usesContextResolverType;
        PreCondition = preCondition;
        Condition = condition;
        NullSubstitute = nullSubstitute;
        Ignored = ignored;
        IsExplicit = isExplicit;
    }
}