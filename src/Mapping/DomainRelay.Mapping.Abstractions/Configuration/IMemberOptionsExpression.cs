using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Converters;
using DomainRelay.Mapping.Abstractions.Resolvers;

namespace DomainRelay.Mapping.Abstractions.Configuration;

/// <summary>
/// Configures how a destination member is mapped.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TDestination">The destination type.</typeparam>
/// <typeparam name="TMember">The destination member type.</typeparam>
public interface IMemberOptionsExpression<TSource, TDestination, TMember>
{
    /// <summary>
    /// Maps the destination member from a source expression.
    /// </summary>
    /// <param name="sourceExpression">The source expression used to compute the destination member value.</param>
    void MapFrom(Expression<Func<TSource, TMember>> sourceExpression);

    /// <summary>
    /// Ignores the destination member.
    /// </summary>
    void Ignore();

    /// <summary>
    /// Applies the member mapping only when the specified predicate returns <see langword="true"/>.
    /// </summary>
    /// <param name="predicate">A predicate receiving the source and destination objects.</param>
    void Condition(Func<TSource, TDestination, bool> predicate);

    /// <summary>
    /// Applies the member mapping only when the specified source predicate returns <see langword="true"/>.
    /// </summary>
    /// <param name="predicate">A predicate receiving the source object.</param>
    void PreCondition(Func<TSource, bool> predicate);

    /// <summary>
    /// Provides a replacement value when the resolved source member value is <see langword="null"/>.
    /// </summary>
    /// <param name="value">The replacement value.</param>
    void NullSubstitute(TMember value);

    /// <summary>
    /// Uses a value converter to transform the source member value.
    /// </summary>
    /// <param name="valueConverter">The converter instance to use.</param>
    void ConvertUsing(IValueConverter<TMember> valueConverter);

    /// <summary>
    /// Uses a resolver instance to compute the destination member value.
    /// </summary>
    /// <param name="resolver">The resolver instance to use.</param>
    void ResolveUsing(IValueResolver<TSource, TDestination, TMember> resolver);

    /// <summary>
    /// Uses a resolver type resolved from dependency injection to compute the destination member value.
    /// </summary>
    /// <typeparam name="TResolver">The resolver type.</typeparam>
    void ResolveUsing<TResolver>()
        where TResolver : class, IValueResolver<TSource, TDestination, TMember>;

    /// <summary>
    /// Uses a context-aware resolver instance to compute the destination member value.
    /// </summary>
    /// <param name="resolver">The context-aware resolver instance to use.</param>
    void ResolveUsing(IContextValueResolver<TSource, TDestination, TMember> resolver);

    /// <summary>
    /// Uses a context-aware resolver type resolved from dependency injection.
    /// </summary>
    /// <typeparam name="TResolver">The context-aware resolver type.</typeparam>
    void ResolveUsingContext<TResolver>()
        where TResolver : class, IContextValueResolver<TSource, TDestination, TMember>;
}