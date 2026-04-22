using System.Linq.Expressions;

namespace DomainRelay.Mapping.Internal;

internal static class ExpressionHelper
{
    public static string GetMemberName(LambdaExpression expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        if (expression.Body is UnaryExpression unaryExpression &&
            unaryExpression.Operand is MemberExpression unaryMemberExpression)
        {
            return unaryMemberExpression.Member.Name;
        }

        throw new InvalidOperationException("Expression must target a member.");
    }
}