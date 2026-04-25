using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Abstractions.Projection;

namespace DomainRelay.Mapping.Expressions.Queryable;

/// <summary>
/// LINQ extensions for applying safe dynamic filters expressed against destination members.
/// </summary>
public static class QueryableDynamicFilterExtensions
{
    public static IQueryable<TSource> WhereTranslatedEquals<TSource, TDestination>(
        this IQueryable<TSource> source,
        string destinationMemberName,
        object? value,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationMemberName);
        ArgumentNullException.ThrowIfNull(translator);

        var predicate = BuildComparisonPredicate<TDestination>(
            destinationMemberName,
            value,
            Expression.Equal);

        return source.WhereTranslated<TSource, TDestination>(predicate, translator);
    }

    public static IQueryable<TSource> WhereTranslatedNotEquals<TSource, TDestination>(
        this IQueryable<TSource> source,
        string destinationMemberName,
        object? value,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationMemberName);
        ArgumentNullException.ThrowIfNull(translator);

        var predicate = BuildComparisonPredicate<TDestination>(
            destinationMemberName,
            value,
            Expression.NotEqual);

        return source.WhereTranslated<TSource, TDestination>(predicate, translator);
    }

    public static IQueryable<TSource> WhereTranslatedGreaterThan<TSource, TDestination>(
        this IQueryable<TSource> source,
        string destinationMemberName,
        object value,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationMemberName);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(translator);

        var predicate = BuildComparisonPredicate<TDestination>(
            destinationMemberName,
            value,
            Expression.GreaterThan);

        return source.WhereTranslated<TSource, TDestination>(predicate, translator);
    }

    public static IQueryable<TSource> WhereTranslatedGreaterThanOrEqual<TSource, TDestination>(
        this IQueryable<TSource> source,
        string destinationMemberName,
        object value,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationMemberName);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(translator);

        var predicate = BuildComparisonPredicate<TDestination>(
            destinationMemberName,
            value,
            Expression.GreaterThanOrEqual);

        return source.WhereTranslated<TSource, TDestination>(predicate, translator);
    }

    public static IQueryable<TSource> WhereTranslatedLessThan<TSource, TDestination>(
        this IQueryable<TSource> source,
        string destinationMemberName,
        object value,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationMemberName);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(translator);

        var predicate = BuildComparisonPredicate<TDestination>(
            destinationMemberName,
            value,
            Expression.LessThan);

        return source.WhereTranslated<TSource, TDestination>(predicate, translator);
    }

    public static IQueryable<TSource> WhereTranslatedLessThanOrEqual<TSource, TDestination>(
        this IQueryable<TSource> source,
        string destinationMemberName,
        object value,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationMemberName);
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(translator);

        var predicate = BuildComparisonPredicate<TDestination>(
            destinationMemberName,
            value,
            Expression.LessThanOrEqual);

        return source.WhereTranslated<TSource, TDestination>(predicate, translator);
    }

    public static IQueryable<TSource> WhereTranslatedStringContains<TSource, TDestination>(
        this IQueryable<TSource> source,
        string destinationMemberName,
        string value,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationMemberName);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        ArgumentNullException.ThrowIfNull(translator);

        var member = ResolveDestinationMember(typeof(TDestination), destinationMemberName);

        if (member.MemberTypeValue != typeof(string))
        {
            throw new ExpressionTranslationException(
                typeof(object),
                typeof(TDestination),
                $"Destination member '{destinationMemberName}' must be a string member to use Contains.",
                destinationMemberName);
        }

        var parameter = Expression.Parameter(typeof(TDestination), "dto");
        var memberAccess = Expression.MakeMemberAccess(parameter, member.MemberInfo);

        var containsMethod = typeof(string).GetMethod(
            nameof(string.Contains),
            new[] { typeof(string) })!;

        var body = Expression.AndAlso(
            Expression.NotEqual(memberAccess, Expression.Constant(null, typeof(string))),
            Expression.Call(memberAccess, containsMethod, Expression.Constant(value)));

        var predicate = Expression.Lambda<Func<TDestination, bool>>(body, parameter);

        return source.WhereTranslated<TSource, TDestination>(predicate, translator);
    }

    private static Expression<Func<TDestination, bool>> BuildComparisonPredicate<TDestination>(
        string destinationMemberName,
        object? value,
        Func<Expression, Expression, BinaryExpression> comparisonFactory)
    {
        var member = ResolveDestinationMember(typeof(TDestination), destinationMemberName);

        var parameter = Expression.Parameter(typeof(TDestination), "dto");
        var memberAccess = Expression.MakeMemberAccess(parameter, member.MemberInfo);

        var convertedValue = ConvertValue(value, member.MemberTypeValue);
        var constant = Expression.Constant(convertedValue, member.MemberTypeValue);

        var body = comparisonFactory(memberAccess, constant);

        return Expression.Lambda<Func<TDestination, bool>>(body, parameter);
    }

    private static ResolvedMember ResolveDestinationMember(Type destinationType, string memberName)
    {
        var member = destinationType
            .GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m =>
                string.Equals(m.Name, memberName, StringComparison.OrdinalIgnoreCase)
                && m.MemberType is MemberTypes.Property or MemberTypes.Field);

        if (member is null)
        {
            throw new ExpressionTranslationException(
                typeof(object),
                destinationType,
                $"Destination member '{memberName}' was not found.",
                memberName);
        }

        var memberType = member switch
        {
            PropertyInfo property => property.PropertyType,
            FieldInfo field => field.FieldType,
            _ => throw new ExpressionTranslationException(
                typeof(object),
                destinationType,
                $"Destination member '{memberName}' is not a property or field.",
                memberName)
        };

        return new ResolvedMember(member, memberType);
    }

    private static object? ConvertValue(object? value, Type destinationType)
    {
        if (value is null)
        {
            if (Nullable.GetUnderlyingType(destinationType) is not null || !destinationType.IsValueType)
                return null;

            throw new ExpressionTranslationException(
                typeof(object),
                destinationType,
                $"Cannot compare non-nullable member type '{destinationType.FullName}' with null.");
        }

        var targetType = Nullable.GetUnderlyingType(destinationType) ?? destinationType;

        if (targetType.IsInstanceOfType(value))
            return value;

        if (targetType.IsEnum)
        {
            if (value is string enumText)
                return Enum.Parse(targetType, enumText, ignoreCase: true);

            return Enum.ToObject(targetType, value);
        }

        if (targetType == typeof(Guid))
        {
            return value is Guid guid
                ? guid
                : Guid.Parse(value.ToString()!);
        }

        if (targetType == typeof(DateTime))
        {
            return value is DateTime dateTime
                ? dateTime
                : DateTime.Parse(value.ToString()!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        if (targetType == typeof(DateTimeOffset))
        {
            return value is DateTimeOffset dateTimeOffset
                ? dateTimeOffset
                : DateTimeOffset.Parse(value.ToString()!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }

    private sealed record ResolvedMember(MemberInfo MemberInfo, Type MemberTypeValue);
}   