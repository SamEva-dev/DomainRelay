using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Projection;

namespace DomainRelay.Mapping.Expressions.Queryable;

public static class QueryableTranslationExtensions
{
    public static IQueryable<TSource> WhereTranslated<TSource, TDestination>(
        this IQueryable<TSource> source,
        Expression<Func<TDestination, bool>> destinationPredicate,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destinationPredicate);
        ArgumentNullException.ThrowIfNull(translator);

        var translated = translator.Translate<TSource, TDestination, bool>(destinationPredicate);
        return source.Where(translated);
    }
}