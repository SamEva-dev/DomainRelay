using System.Linq.Expressions;

namespace DomainRelay.Mapping.Abstractions.Projection;

public interface IExpressionTranslator
{
    Expression<Func<TSource, TResult>> Translate<TSource, TDestination, TResult>(
        Expression<Func<TDestination, TResult>> destinationExpression);

    LambdaExpression Translate(
        LambdaExpression destinationExpression,
        Type sourceType,
        Type destinationType);
}