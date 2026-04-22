using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Projection;

namespace DomainRelay.Mapping.Expressions.Translation;

internal sealed class DestinationToSourceExpressionTranslator : IExpressionTranslator
{
    private readonly ExpressionTranslationPlanBuilder _planBuilder;
    private readonly ExpressionTranslationValidator _validator;

    public DestinationToSourceExpressionTranslator(
        ExpressionTranslationPlanBuilder planBuilder,
        ExpressionTranslationValidator validator)
    {
        _planBuilder = planBuilder;
        _validator = validator;
    }

    public Expression<Func<TSource, TResult>> Translate<TSource, TDestination, TResult>(
        Expression<Func<TDestination, TResult>> destinationExpression)
    {
        var lambda = Translate(destinationExpression, typeof(TSource), typeof(TDestination));
        return (Expression<Func<TSource, TResult>>)lambda;
    }

    public LambdaExpression Translate(
        LambdaExpression destinationExpression,
        Type sourceType,
        Type destinationType)
    {
        _validator.Validate(destinationExpression);

        var plan = _planBuilder.Build(sourceType, destinationType);

        var sourceParameter = Expression.Parameter(sourceType, "src");
        var destinationParameter = destinationExpression.Parameters[0];

        var visitor = new DestinationExpressionVisitor(destinationParameter, sourceParameter, plan);
        var translatedBody = visitor.Visit(destinationExpression.Body)!;

        var delegateType = typeof(Func<,>).MakeGenericType(sourceType, destinationExpression.ReturnType);
        return Expression.Lambda(delegateType, translatedBody, sourceParameter);
    }
}