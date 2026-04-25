using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Projection;

namespace DomainRelay.Mapping.Expressions.Queryable;

public static class QueryableOrderExtensions
{
    public static IOrderedQueryable<TSource> OrderByTranslated<TSource, TDestination, TKey>(
        this IQueryable<TSource> source,
        Expression<Func<TDestination, TKey>> destinationKeySelector,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destinationKeySelector);
        ArgumentNullException.ThrowIfNull(translator);

        var translated = translator.Translate<TSource, TDestination, TKey>(destinationKeySelector);
        return source.OrderBy(translated);
    }

    public static IOrderedQueryable<TSource> OrderByDescendingTranslated<TSource, TDestination, TKey>(
        this IQueryable<TSource> source,
        Expression<Func<TDestination, TKey>> destinationKeySelector,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destinationKeySelector);
        ArgumentNullException.ThrowIfNull(translator);

        var translated = translator.Translate<TSource, TDestination, TKey>(destinationKeySelector);
        return source.OrderByDescending(translated);
    }

    public static IOrderedQueryable<TSource> ThenByTranslated<TSource, TDestination, TKey>(
        this IOrderedQueryable<TSource> source,
        Expression<Func<TDestination, TKey>> destinationKeySelector,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destinationKeySelector);
        ArgumentNullException.ThrowIfNull(translator);

        var translated = translator.Translate<TSource, TDestination, TKey>(destinationKeySelector);
        return source.ThenBy(translated);
    }

    public static IOrderedQueryable<TSource> ThenByDescendingTranslated<TSource, TDestination, TKey>(
        this IOrderedQueryable<TSource> source,
        Expression<Func<TDestination, TKey>> destinationKeySelector,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destinationKeySelector);
        ArgumentNullException.ThrowIfNull(translator);

        var translated = translator.Translate<TSource, TDestination, TKey>(destinationKeySelector);
        return source.ThenByDescending(translated);
    }
}