using System.Reflection;

namespace DomainRelay.Mapping.Resolution;

internal static class FlatteningResolver
{
    public static Func<object, object?>? TryBuildResolver(Type sourceType, string destinationMemberName)
    {
        var path = TryResolvePath(sourceType, destinationMemberName);
        if (path is null || path.Count == 0)
        {
            return null;
        }

        return source =>
        {
            object? current = source;

            foreach (var property in path)
            {
                if (current is null)
                {
                    return null;
                }

                current = property.GetValue(current);
            }

            return current;
        };
    }

    private static List<PropertyInfo>? TryResolvePath(Type sourceType, string destinationMemberName)
    {
        return TryResolvePathRecursive(sourceType, destinationMemberName, new List<PropertyInfo>(), 0);
    }

    private static List<PropertyInfo>? TryResolvePathRecursive(
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

                var nested = TryResolvePathRecursive(property.PropertyType, nextRemaining, nextPath, depth + 1);
                if (nested is not null)
                {
                    return nested;
                }
            }
        }

        return null;
    }
}