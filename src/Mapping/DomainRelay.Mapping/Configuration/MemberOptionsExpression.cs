using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Converters;
using DomainRelay.Mapping.Abstractions.Resolvers;
using DomainRelay.Mapping.Abstractions.Services;

namespace DomainRelay.Mapping.Configuration;

internal sealed class MemberOptionsExpression<TSource, TDestination, TMember>
    : IMemberOptionsExpression<TSource, TDestination, TMember>
{
    private readonly string _destinationMemberName;

    private Expression<Func<TSource, TMember>>? _sourceExpression;
    private Func<TSource, TDestination, TMember>? _sourceResolver;
    private Func<TSource, TDestination, IMappingContext, TMember>? _contextSourceResolver;
    private Type? _resolverType;
    private bool _usesContextResolverType;
    private Func<TSource, bool>? _preCondition;
    private Func<TSource, TDestination, bool>? _condition;
    private TMember? _nullSubstitute;
    private bool _ignored;
    private IValueConverter<TMember>? _valueConverter;

    public MemberOptionsExpression(string destinationMemberName)
    {
        _destinationMemberName = destinationMemberName;
    }

    public void MapFrom(Expression<Func<TSource, TMember>> sourceExpression)
    {
        ArgumentNullException.ThrowIfNull(sourceExpression);
        _sourceExpression = sourceExpression;
    }

    public void Ignore()
    {
        _ignored = true;
    }

    public void Condition(Func<TSource, TDestination, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        _condition = predicate;
    }

    public void PreCondition(Func<TSource, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        _preCondition = predicate;
    }

    public void NullSubstitute(TMember value)
    {
        _nullSubstitute = value;
    }

    public void ConvertUsing(IValueConverter<TMember> valueConverter)
    {
        ArgumentNullException.ThrowIfNull(valueConverter);
        _valueConverter = valueConverter;
    }

    public void ResolveUsing(IValueResolver<TSource, TDestination, TMember> resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        _sourceResolver = resolver.Resolve;
        _contextSourceResolver = null;
        _resolverType = null;
        _usesContextResolverType = false;
    }

    public void ResolveUsing<TResolver>()
        where TResolver : class, IValueResolver<TSource, TDestination, TMember>
    {
        _resolverType = typeof(TResolver);
        _usesContextResolverType = false;
        _sourceResolver = null;
        _contextSourceResolver = null;
    }

    public void ResolveUsing(IContextValueResolver<TSource, TDestination, TMember> resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        _contextSourceResolver = resolver.Resolve;
        _sourceResolver = null;
        _resolverType = null;
        _usesContextResolverType = false;
    }

    public void ResolveUsingContext<TResolver>()
        where TResolver : class, IContextValueResolver<TSource, TDestination, TMember>
    {
        _resolverType = typeof(TResolver);
        _usesContextResolverType = true;
        _sourceResolver = null;
        _contextSourceResolver = null;
    }

    public MemberMapDefinition Build()
    {
        Func<object, object, object?>? boxedResolver = null;
        if (_sourceResolver is not null)
        {
            boxedResolver = (source, destination) => _sourceResolver((TSource)source, (TDestination)destination);
        }

        Func<object, object, IMappingContext, object?>? boxedContextResolver = null;
        if (_contextSourceResolver is not null)
        {
            boxedContextResolver = (source, destination, context) =>
                _contextSourceResolver((TSource)source, (TDestination)destination, context);
        }

        Func<object, bool>? boxedPreCondition = null;
        if (_preCondition is not null)
        {
            boxedPreCondition = source => _preCondition((TSource)source);
        }

        Func<object, object, bool>? boxedCondition = null;
        if (_condition is not null)
        {
            boxedCondition = (source, destination) => _condition((TSource)source, (TDestination)destination);
        }

        return new MemberMapDefinition(
            _destinationMemberName,
            _sourceExpression,
            boxedResolver,
            boxedContextResolver,
            _resolverType,
            _usesContextResolverType,
            boxedPreCondition,
            boxedCondition,
            _nullSubstitute,
            _ignored,
            _valueConverter);
    }
}