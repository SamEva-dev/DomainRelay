using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.Expressions.Translation;

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

        try
        {
            var translated = translator.Translate<TSource, TDestination, bool>(destinationPredicate);
            return source.Where(translated);
        }
        catch (TranslationValidationException ex)
        {
            throw new ExpressionTranslationException(
                typeof(TSource),
                typeof(TDestination),
                ex.Message,
                ex,
                destinationPredicate.ToString());
        }
    }
}
