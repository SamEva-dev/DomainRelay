using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Converters;
using DomainRelay.Mapping.Abstractions.Resolvers;

namespace DomainRelay.Mapping.Abstractions.Configuration;

public interface IMemberOptionsExpression<TSource, TDestination, TMember>
{
    void MapFrom(Expression<Func<TSource, TMember>> sourceExpression);

    void Ignore();

    void Condition(Func<TSource, TDestination, bool> predicate);

    void PreCondition(Func<TSource, bool> predicate);

    void NullSubstitute(TMember value);

    void ConvertUsing(IValueConverter<TMember> valueConverter);

    void ResolveUsing(IValueResolver<TSource, TDestination, TMember> resolver);

    void ResolveUsing<TResolver>()
        where TResolver : class, IValueResolver<TSource, TDestination, TMember>;

    void ResolveUsing(IContextValueResolver<TSource, TDestination, TMember> resolver);

    void ResolveUsingContext<TResolver>()
        where TResolver : class, IContextValueResolver<TSource, TDestination, TMember>;
}