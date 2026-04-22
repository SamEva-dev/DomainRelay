using System.Linq.Expressions;
using System.Reflection;

namespace DomainRelay.Mapping.Expressions.Projection;

internal static class ProjectionFlatteningResolver
{
    public static Expression? TryBuildExpression(Type sourceType, string destinationMemberName)
    {
        var sourceParameter = Expression.Parameter(sourceType, "src");
        return TryBuildExpression(sourceType, destinationMemberName, sourceParameter);
    }

    public static Expression? TryBuildExpression(Type sourceType, string destinationMemberName, Expression sourceRoot)
    {
        var path = TryResolvePath(sourceType, destinationMemberName, new List<PropertyInfo>(), 0);
        if (path is null || path.Count == 0)
        {
            return null;
        }

        Expression current = sourceRoot;
        foreach (var property in path)
        {
            current = Expression.Property(current, property);
        }

        return current;
    }

    private static List<PropertyInfo>? TryResolvePath(
        Type currentType,
        string remainingName,
        List<PropertyInfo> currentPath,
        int depth)
    {
        if (depth > 5)
        {
            return null;
        }

        foreach (var property in currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead)
            {
                continue;
            }

            if (remainingName.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
            {
                var result = new List<PropertyInfo>(currentPath) { property };
                return result;
            }

            if (remainingName.StartsWith(property.Name, StringComparison.OrdinalIgnoreCase))
            {
                var nextRemaining = remainingName[property.Name.Length..];
                var nextPath = new List<PropertyInfo>(currentPath) { property };

                var nested = TryResolvePath(property.PropertyType, nextRemaining, nextPath, depth + 1);
                if (nested is not null)
                {
                    return nested;
                }
            }
        }

        return null;
    }
}