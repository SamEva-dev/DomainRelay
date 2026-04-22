using System.Collections;
using System.Reflection;

namespace DomainRelay.Mapping.Collections;

internal sealed class DictionaryMapper : IDictionaryMapper
{
    public bool CanMap(Type sourceType, Type destinationType)
    {
        return IsObjectToDictionary(destinationType)
               || IsDictionaryToObject(sourceType, destinationType);
    }

    public object? MapDictionary(
        object source,
        object? destination,
        Type sourceType,
        Type destinationType,
        Func<object, Type, Type, object?> nestedMap)
    {
        if (IsObjectToDictionary(destinationType))
        {
            IDictionary result = destination as IDictionary
                                 ?? new Dictionary<string, object?>();

            result.Clear();

            var properties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead);

            foreach (var property in properties)
            {
                var value = property.GetValue(source);

                if (value is not null && !IsSimpleValue(value.GetType()))
                {
                    result[property.Name] = nestedMap(value, value.GetType(), value.GetType());
                }
                else
                {
                    result[property.Name] = value;
                }
            }

            return result;
        }

        if (IsDictionaryToObject(sourceType, destinationType))
        {
            var dictionary = (IDictionary)source;
            var target = destination ?? Activator.CreateInstance(destinationType)!;

            var properties = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key is not string key)
                {
                    continue;
                }

                if (!properties.TryGetValue(key, out var property))
                {
                    continue;
                }

                if (entry.Value is null)
                {
                    if (!property.PropertyType.IsValueType ||
                        Nullable.GetUnderlyingType(property.PropertyType) is not null)
                    {
                        property.SetValue(target, null);
                    }

                    continue;
                }

                if (property.PropertyType.IsAssignableFrom(entry.Value.GetType()))
                {
                    property.SetValue(target, entry.Value);
                    continue;
                }

                var nested = nestedMap(entry.Value, entry.Value.GetType(), property.PropertyType);
                property.SetValue(target, nested);
            }

            return target;
        }

        return null;
    }

    private static bool IsObjectToDictionary(Type destinationType)
    {
        return destinationType == typeof(Dictionary<string, object?>)
               || typeof(IDictionary).IsAssignableFrom(destinationType);
    }

    private static bool IsDictionaryToObject(Type sourceType, Type destinationType)
    {
        return typeof(IDictionary).IsAssignableFrom(sourceType)
               && destinationType != typeof(Dictionary<string, object?>)
               && !typeof(IDictionary).IsAssignableFrom(destinationType);
    }

    private static bool IsSimpleValue(Type type)
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
}