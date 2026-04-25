using System.Linq.Expressions;

namespace DomainRelay.Mapping.Abstractions.Projection;

/// <summary>
/// Translates expressions written against a destination type into expressions written against a source type.
/// </summary>
/// <remarks>
/// This service is used by <c>WhereTranslated</c>, <c>OrderByTranslated</c> and similar query extensions.
/// It enables callers to express filters and sorting using DTO members while applying them to source entities.
/// </remarks>
/// <example>
/// <code>
/// Expression&lt;Func&lt;AuditLogDto, bool&gt;&gt; dtoFilter = dto =&gt; dto.StatusCode &gt;= 400;
/// var entityFilter = translator.Translate&lt;AuditLog, AuditLogDto, bool&gt;(dtoFilter);
/// </code>
/// </example>
public interface IExpressionTranslator
{
    /// <summary>
    /// Translates a destination expression into a source expression.
    /// </summary>
    /// <typeparam name="TSource">The source type to translate to.</typeparam>
    /// <typeparam name="TDestination">The destination type the expression is written against.</typeparam>
    /// <typeparam name="TResult">The expression result type.</typeparam>
    /// <param name="destinationExpression">The expression written against the destination type.</param>
    /// <returns>An equivalent expression written against the source type.</returns>
    Expression<Func<TSource, TResult>> Translate<TSource, TDestination, TResult>(
        Expression<Func<TDestination, TResult>> destinationExpression);

    /// <summary>
    /// Translates a destination expression into a source expression using runtime types.
    /// </summary>
    /// <param name="destinationExpression">The expression written against the destination type.</param>
    /// <param name="sourceType">The source type to translate to.</param>
    /// <param name="destinationType">The destination type the expression is written against.</param>
    /// <returns>An equivalent source expression.</returns>
    LambdaExpression Translate(
        LambdaExpression destinationExpression,
        Type sourceType,
        Type destinationType);
}