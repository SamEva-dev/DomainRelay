namespace DomainRelay.Mapping.Internal;

internal static class TypeHelper
{
    public static bool IsSimpleType(Type type)
    {
        var actualType = Nullable.GetUnderlyingType(type) ?? type;

        return actualType.IsPrimitive
               || actualType.IsEnum
               || actualType == typeof(string)
               || actualType == typeof(decimal)
               || actualType == typeof(DateTime)
               || actualType == typeof(DateTimeOffset)
               || actualType == typeof(Guid)
               || actualType == typeof(TimeSpan);
    }

    public static bool IsDictionary(Type type)
    {
        return typeof(System.Collections.IDictionary).IsAssignableFrom(type)
               || type.GetInterfaces().Any(i =>
                   i.IsGenericType &&
                   i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }

    public static bool IsEnumerable(Type type)
    {
        return type != typeof(string)
               && typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    public static bool IsGenericEnumerable(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    }

    public static Type? TryGetEnumerableElementType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return type.GetGenericArguments()[0];
        }

        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableInterface?.GetGenericArguments()[0];
    }
}