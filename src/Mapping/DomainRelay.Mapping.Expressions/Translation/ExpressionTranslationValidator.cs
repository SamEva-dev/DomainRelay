using System.Linq.Expressions;

namespace DomainRelay.Mapping.Expressions.Translation;

internal sealed class ExpressionTranslationValidator : ExpressionVisitor
{
    public void Validate(LambdaExpression expression)
    {
        if (expression.Parameters.Count != 1)
        {
            throw new TranslationValidationException("Translated expressions must have exactly one parameter.");
        }

        Visit(expression.Body);
    }

    protected override Expression VisitInvocation(InvocationExpression node)
    {
        throw new TranslationValidationException("Invocation expressions are not supported in translated expressions.");
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (IsAllowedStringMethod(node) || IsAllowedContainsMethod(node))
        {
            return base.VisitMethodCall(node);
        }

        throw new TranslationValidationException(
            $"Method '{node.Method.Name}' declared on '{node.Method.DeclaringType?.FullName}' is not supported for translation.");
    }

    private static bool IsAllowedStringMethod(MethodCallExpression node)
    {
        if (node.Method.DeclaringType != typeof(string))
        {
            return false;
        }

        return node.Method.Name is nameof(string.Contains)
            or nameof(string.StartsWith)
            or nameof(string.EndsWith)
            or nameof(string.ToLower)
            or nameof(string.ToUpper)
            or nameof(string.Trim);
    }

    private static bool IsAllowedContainsMethod(MethodCallExpression node)
    {
        if (node.Method.Name != "Contains")
        {
            return false;
        }

        if (node.Method.DeclaringType == typeof(string))
        {
            return true;
        }

        return node.Method.DeclaringType == typeof(Enumerable)
               || node.Method.DeclaringType == typeof(System.Linq.Queryable)
               || typeof(System.Collections.IEnumerable).IsAssignableFrom(node.Method.DeclaringType!);
    }
}