using System.Linq.Expressions;
using System.Linq;
using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Abstractions.Projection;

namespace DomainRelay.Mapping.Expressions.Queryable;

public static class QueryableOrderExtensions
{
    public static IOrderedQueryable<TSource> OrderByTranslated<TSource, TDestination, TKey>(
        this IQueryable<TSource> source,
        Expression<Func<TDestination, TKey>> destinationKeySelector,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destinationKeySelector);
        ArgumentNullException.ThrowIfNull(translator);

        var translated = translator.Translate<TSource, TDestination, TKey>(destinationKeySelector);
        return source.OrderBy(translated);
    }

    public static IOrderedQueryable<TSource> OrderByDescendingTranslated<TSource, TDestination, TKey>(
        this IQueryable<TSource> source,
        Expression<Func<TDestination, TKey>> destinationKeySelector,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destinationKeySelector);
        ArgumentNullException.ThrowIfNull(translator);

        var translated = translator.Translate<TSource, TDestination, TKey>(destinationKeySelector);
        return source.OrderByDescending(translated);
    }

    public static IOrderedQueryable<TSource> ThenByTranslated<TSource, TDestination, TKey>(
        this IOrderedQueryable<TSource> source,
        Expression<Func<TDestination, TKey>> destinationKeySelector,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destinationKeySelector);
        ArgumentNullException.ThrowIfNull(translator);

        var translated = translator.Translate<TSource, TDestination, TKey>(destinationKeySelector);
        return source.ThenBy(translated);
    }

    public static IOrderedQueryable<TSource> ThenByDescendingTranslated<TSource, TDestination, TKey>(
        this IOrderedQueryable<TSource> source,
        Expression<Func<TDestination, TKey>> destinationKeySelector,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destinationKeySelector);
        ArgumentNullException.ThrowIfNull(translator);

        var translated = translator.Translate<TSource, TDestination, TKey>(destinationKeySelector);
        return source.ThenByDescending(translated);
    }

    public static IOrderedQueryable<TSource> OrderByTranslated<TSource, TDestination>(
        this IQueryable<TSource> source,
        string destinationMemberName,
        bool descending,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationMemberName);
        ArgumentNullException.ThrowIfNull(translator);

        var keySelector = BuildDestinationMemberSelector<TDestination>(destinationMemberName);

        var translated = translator.Translate(
            keySelector,
            typeof(TSource),
            typeof(TDestination));

        return ApplyOrderBy(source, translated, descending);
    }

    public static IOrderedQueryable<TSource> ThenByTranslated<TSource, TDestination>(
        this IOrderedQueryable<TSource> source,
        string destinationMemberName,
        bool descending,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationMemberName);
        ArgumentNullException.ThrowIfNull(translator);

        var keySelector = BuildDestinationMemberSelector<TDestination>(destinationMemberName);

        var translated = translator.Translate(
            keySelector,
            typeof(TSource),
            typeof(TDestination));

        return ApplyThenBy(source, translated, descending);
    }

    private static LambdaExpression BuildDestinationMemberSelector<TDestination>(string destinationMemberName)
    {
        var destinationType = typeof(TDestination);

        var member = destinationType
            .GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .FirstOrDefault(m =>
        string.Equals(m.Name, destinationMemberName, StringComparison.OrdinalIgnoreCase)
        && m.MemberType is System.Reflection.MemberTypes.Property or System.Reflection.MemberTypes.Field);

        if (member is null)
        {
            throw new ExpressionTranslationException(
                sourceType: typeof(object),
                destinationType: destinationType,
                message: $"Destination member '{destinationMemberName}' was not found.",
                expressionText: destinationMemberName);
        }

        var parameter = Expression.Parameter(destinationType, "dto");

        Expression body = member.MemberType switch
        {
            System.Reflection.MemberTypes.Property => Expression.Property(parameter, (System.Reflection.PropertyInfo)member),
            System.Reflection.MemberTypes.Field => Expression.Field(parameter, (System.Reflection.FieldInfo)member),
            _ => throw new ExpressionTranslationException(
                typeof(object),
                destinationType,
                $"Destination member '{destinationMemberName}' is not a property or field.",
                destinationMemberName)
        };

        var delegateType = typeof(Func<,>).MakeGenericType(destinationType, body.Type);

        return Expression.Lambda(delegateType, body, parameter);
    }

    private static IOrderedQueryable<TSource> ApplyOrderBy<TSource>(
        IQueryable<TSource> source,
        LambdaExpression keySelector,
        bool descending)
    {
        var methodName = descending
            ? nameof(System.Linq.Queryable.OrderByDescending)
            : nameof(System.Linq.Queryable.OrderBy);

        return ApplyOrderingMethod<TSource>(
            source,
            keySelector,
            methodName);
    }

    private static IOrderedQueryable<TSource> ApplyThenBy<TSource>(
        IOrderedQueryable<TSource> source,
        LambdaExpression keySelector,
        bool descending)
    {
        var methodName = descending
            ? nameof(System.Linq.Queryable.ThenByDescending)
            : nameof(System.Linq.Queryable.ThenBy);

        return ApplyOrderingMethod<TSource>(
            source,
            keySelector,
            methodName);
    }

    private static IOrderedQueryable<TSource> ApplyOrderingMethod<TSource>(
        IQueryable<TSource> source,
        LambdaExpression keySelector,
        string methodName)
    {
        var sourceType = typeof(TSource);
        var keyType = keySelector.ReturnType;

        var call = Expression.Call(
            typeof(System.Linq.Queryable),
            methodName,
            new[] { sourceType, keyType },
            source.Expression,
            Expression.Quote(keySelector));

        return (IOrderedQueryable<TSource>)source.Provider.CreateQuery<TSource>(call);
    }
}