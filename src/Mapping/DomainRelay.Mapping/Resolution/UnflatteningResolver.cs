using System.Reflection;

namespace DomainRelay.Mapping.Resolution;

internal static class UnflatteningResolver
{
    public static bool TryAssign(object destination, string sourceMemberName, object? value)
    {
        return TryAssignRecursive(destination, destination.GetType(), sourceMemberName, value, 0);
    }

    private static bool TryAssignRecursive(object currentObject, Type currentType, string remainingName, object? value, int depth)
    {
        if (depth > 5)
        {
            return false;
        }

        foreach (var property in currentType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (remainingName.Equals(property.Name, StringComparison.OrdinalIgnoreCase) && property.CanWrite)
            {
                if (value is null)
                {
                    if (!property.PropertyType.IsValueType ||
                        Nullable.GetUnderlyingType(property.PropertyType) is not null)
                    {
                        property.SetValue(currentObject, null);
                        return true;
                    }

                    return false;
                }

                if (property.PropertyType.IsAssignableFrom(value.GetType()))
                {
                    property.SetValue(currentObject, value);
                    return true;
                }

                return false;
            }

            if (remainingName.StartsWith(property.Name, StringComparison.OrdinalIgnoreCase))
            {
                var nextRemaining = remainingName[property.Name.Length..];
                if (string.IsNullOrWhiteSpace(nextRemaining))
                {
                    continue;
                }

                var nestedValue = property.GetValue(currentObject);
                if (nestedValue is null)
                {
                    if (property.PropertyType.GetConstructor(Type.EmptyTypes) is null)
                    {
                        continue;
                    }

                    nestedValue = Activator.CreateInstance(property.PropertyType)!;
                    property.SetValue(currentObject, nestedValue);
                }

                if (TryAssignRecursive(nestedValue, property.PropertyType, nextRemaining, value, depth + 1))
                {
                    return true;
                }
            }
        }

        return false;
    }
}