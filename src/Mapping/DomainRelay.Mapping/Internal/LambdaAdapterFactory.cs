using System.Linq.Expressions;

namespace DomainRelay.Mapping.Internal;

internal static class LambdaAdapterFactory
{
    public static Func<object, object?> AdaptSourceResolver(LambdaExpression lambda)
    {
        var sourceParameter = Expression.Parameter(typeof(object), "source");

        var castedSource = Expression.Convert(sourceParameter, lambda.Parameters[0].Type);
        var invoked = Expression.Invoke(lambda, castedSource);
        var boxedResult = Expression.Convert(invoked, typeof(object));

        var wrapper = Expression.Lambda<Func<object, object?>>(boxedResult, sourceParameter);
        return wrapper.Compile();
    }

    public static Func<object, object, bool> AdaptCondition(Delegate conditionDelegate, Type sourceType, Type destinationType)
    {
        var sourceParameter = Expression.Parameter(typeof(object), "source");
        var destinationParameter = Expression.Parameter(typeof(object), "destination");

        var castedSource = Expression.Convert(sourceParameter, sourceType);
        var castedDestination = Expression.Convert(destinationParameter, destinationType);

        var conditionExpression = Expression.Invoke(
            Expression.Constant(conditionDelegate),
            castedSource,
            castedDestination);

        var wrapper = Expression.Lambda<Func<object, object, bool>>(
            conditionExpression,
            sourceParameter,
            destinationParameter);

        return wrapper.Compile();
    }
}