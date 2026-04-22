using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace DomainRelay.Mapping.Cache;

internal sealed class MemberAccessorCache
{
    private readonly ConcurrentDictionary<PropertyInfo, Func<object, object?>> _getters = new();
    private readonly ConcurrentDictionary<PropertyInfo, Action<object, object?>> _setters = new();

    public Func<object, object?> GetGetter(PropertyInfo propertyInfo)
    {
        return _getters.GetOrAdd(propertyInfo, BuildGetter);
    }

    public Action<object, object?> GetSetter(PropertyInfo propertyInfo)
    {
        return _setters.GetOrAdd(propertyInfo, BuildSetter);
    }

    private static Func<object, object?> BuildGetter(PropertyInfo propertyInfo)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var casted = Expression.Convert(instance, propertyInfo.DeclaringType!);
        var property = Expression.Property(casted, propertyInfo);
        var boxed = Expression.Convert(property, typeof(object));

        return Expression.Lambda<Func<object, object?>>(boxed, instance).Compile();
    }

    private static Action<object, object?> BuildSetter(PropertyInfo propertyInfo)
    {
        var instance = Expression.Parameter(typeof(object), "instance");
        var value = Expression.Parameter(typeof(object), "value");

        var castedInstance = Expression.Convert(instance, propertyInfo.DeclaringType!);
        var castedValue = Expression.Convert(value, propertyInfo.PropertyType);

        var assign = Expression.Assign(Expression.Property(castedInstance, propertyInfo), castedValue);

        return Expression.Lambda<Action<object, object?>>(assign, instance, value).Compile();
    }
}