using System.Linq.Expressions;

namespace DomainRelay.Mapping.Expressions.Translation;

internal sealed class DestinationExpressionVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _destinationParameter;
    private readonly Expression _sourceRoot;
    private readonly ExpressionTranslationPlan _plan;

    public DestinationExpressionVisitor(
        ParameterExpression destinationParameter,
        Expression sourceRoot,
        ExpressionTranslationPlan plan)
    {
        _destinationParameter = destinationParameter;
        _sourceRoot = sourceRoot;
        _plan = plan;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (node == _destinationParameter)
        {
            return _sourceRoot;
        }

        return base.VisitParameter(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression == _destinationParameter)
        {
            if (_plan.Members.TryGetValue(node.Member.Name, out var translationMap))
            {
                return ParameterReplaceVisitor.Replace(
                    translationMap.SourceExpressionBody,
                    ExpressionParameterFinder.FindRootParameter(translationMap.SourceExpressionBody),
                    _sourceRoot);
            }

            throw new TranslationValidationException(
                $"Destination member '{_plan.DestinationType.FullName}.{node.Member.Name}' cannot be translated to source '{_plan.SourceType.FullName}'.");
        }

        var visitedExpression = node.Expression is null ? null : Visit(node.Expression);
        if (visitedExpression != node.Expression)
        {
            return Expression.MakeMemberAccess(visitedExpression, node.Member);
        }

        return base.VisitMember(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(string) &&
            (node.Method.Name == nameof(string.Contains)
             || node.Method.Name == nameof(string.StartsWith)
             || node.Method.Name == nameof(string.EndsWith)))
        {
            var instance = node.Object is null ? null : Visit(node.Object);
            var arguments = node.Arguments.Select(a => Visit(a)!).ToArray();
            return Expression.Call(instance, node.Method, arguments);
        }

        if (node.Method.Name == "Contains")
        {
            var instance = node.Object is null ? null : Visit(node.Object);
            var arguments = node.Arguments.Select(a => Visit(a)!).ToArray();
            return Expression.Call(instance, node.Method, arguments);
        }

        return base.VisitMethodCall(node);
    }
}
