using System.Linq.Expressions;

namespace DomainRelay.Mapping.Expressions.Translation;

internal sealed class ParameterReplaceVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _source;
    private readonly Expression _target;

    private ParameterReplaceVisitor(ParameterExpression source, Expression target)
    {
        _source = source;
        _target = target;
    }

    public static Expression Replace(Expression expression, ParameterExpression source, Expression target)
    {
        return new ParameterReplaceVisitor(source, target).Visit(expression)!;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (node == _source)
        {
            return _target;
        }

        return base.VisitParameter(node);
    }
}