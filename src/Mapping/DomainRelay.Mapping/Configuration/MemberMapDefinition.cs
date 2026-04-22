using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Services;

namespace DomainRelay.Mapping.Configuration;

internal sealed class MemberMapDefinition
{
    public string DestinationMemberName { get; }
    public LambdaExpression? SourceExpression { get; }
    public Func<object, object, object?>? SourceResolver { get; }
    public Func<object, object, IMappingContext, object?>? ContextSourceResolver { get; }
    public Type? ResolverType { get; }
    public bool UsesContextResolverType { get; }
    public Func<object, bool>? PreCondition { get; }
    public Func<object, object, bool>? Condition { get; }
    public object? NullSubstitute { get; }
    public bool Ignored { get; }
    public object? ValueConverter { get; }

    public MemberMapDefinition(
        string destinationMemberName,
        LambdaExpression? sourceExpression,
        Func<object, object, object?>? sourceResolver,
        Func<object, object, IMappingContext, object?>? contextSourceResolver,
        Type? resolverType,
        bool usesContextResolverType,
        Func<object, bool>? preCondition,
        Func<object, object, bool>? condition,
        object? nullSubstitute,
        bool ignored,
        object? valueConverter)
    {
        DestinationMemberName = destinationMemberName;
        SourceExpression = sourceExpression;
        SourceResolver = sourceResolver;
        ContextSourceResolver = contextSourceResolver;
        ResolverType = resolverType;
        UsesContextResolverType = usesContextResolverType;
        PreCondition = preCondition;
        Condition = condition;
        NullSubstitute = nullSubstitute;
        Ignored = ignored;
        ValueConverter = valueConverter;
    }

    public static MemberMapDefinition CreateIgnored(string destinationMemberName)
    {
        return new MemberMapDefinition(
            destinationMemberName,
            sourceExpression: null,
            sourceResolver: null,
            contextSourceResolver: null,
            resolverType: null,
            usesContextResolverType: false,
            preCondition: null,
            condition: null,
            nullSubstitute: null,
            ignored: true,
            valueConverter: null);
    }
}