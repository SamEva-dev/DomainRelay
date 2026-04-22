using System.Linq.Expressions;

namespace DomainRelay.Mapping.Expressions.Translation;

internal static class ExpressionParameterFinder
{
    public static ParameterExpression FindRootParameter(Expression expression)
    {
        var finder = new RootParameterVisitor();
        finder.Visit(expression);

        return finder.Parameter
               ?? throw new InvalidOperationException("Unable to find root parameter in expression.");
    }

    private sealed class RootParameterVisitor : ExpressionVisitor
    {
        public ParameterExpression? Parameter { get; private set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Parameter ??= node;
            return base.VisitParameter(node);
        }
    }
}
