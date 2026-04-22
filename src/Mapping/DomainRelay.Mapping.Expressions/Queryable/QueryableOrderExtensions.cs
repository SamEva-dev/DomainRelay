using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Projection;

namespace DomainRelay.Mapping.Expressions.Queryable;

public static class QueryableOrderExtensions
{
    public static IQueryable<TSource> OrderByTranslated<TSource, TDestination, TKey>(
        this IQueryable<TSource> source,
        Expression<Func<TDestination, TKey>> destinationKeySelector,
        IExpressionTranslator translator)
    {
        var translated = translator.Translate<TSource, TDestination, TKey>(destinationKeySelector);
        return source.OrderBy(translated);
    }
}