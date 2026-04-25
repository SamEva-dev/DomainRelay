using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.Expressions.Dynamic;

namespace DomainRelay.Mapping.Expressions.Queryable;

/// <summary>
/// LINQ extensions for applying safe dynamic query options expressed against destination members.
/// </summary>
public static class QueryableDynamicQueryExtensions
{
    /// <summary>
    /// Applies all dynamic filters to a source query.
    /// </summary>
    public static IQueryable<TSource> ApplyDynamicFilters<TSource, TDestination>(
        this IQueryable<TSource> source,
        IEnumerable<DynamicFilter> filters,
        IExpressionTranslator translator)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filters);
        ArgumentNullException.ThrowIfNull(translator);

        var query = source;

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.MemberName))
                continue;

            query = filter.Operator switch
            {
                DynamicFilterOperator.Equals =>
                    query.WhereTranslatedEquals<TSource, TDestination>(
                        filter.MemberName,
                        filter.Value,
                        translator),

                DynamicFilterOperator.NotEquals =>
                    query.WhereTranslatedNotEquals<TSource, TDestination>(
                        filter.MemberName,
                        filter.Value,
                        translator),

                DynamicFilterOperator.GreaterThan =>
                    query.WhereTranslatedGreaterThan<TSource, TDestination>(
                        filter.MemberName,
                        RequireValue(filter),
                        translator),

                DynamicFilterOperator.GreaterThanOrEqual =>
                    query.WhereTranslatedGreaterThanOrEqual<TSource, TDestination>(
                        filter.MemberName,
                        RequireValue(filter),
                        translator),

                DynamicFilterOperator.LessThan =>
                    query.WhereTranslatedLessThan<TSource, TDestination>(
                        filter.MemberName,
                        RequireValue(filter),
                        translator),

                DynamicFilterOperator.LessThanOrEqual =>
                    query.WhereTranslatedLessThanOrEqual<TSource, TDestination>(
                        filter.MemberName,
                        RequireValue(filter),
                        translator),

                DynamicFilterOperator.StringContains =>
                    query.WhereTranslatedStringContains<TSource, TDestination>(
                        filter.MemberName,
                        RequireStringValue(filter),
                        translator),

                _ => query
            };
        }

        return query;
    }

    /// <summary>
    /// Applies all dynamic sorts to a source query.
    /// </summary>
    public static IOrderedQueryable<TSource> ApplyDynamicSorts<TSource, TDestination>(
        this IQueryable<TSource> source,
        IEnumerable<DynamicSort> sorts,
        IExpressionTranslator translator,
        DynamicSort fallbackSort)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sorts);
        ArgumentNullException.ThrowIfNull(translator);
        ArgumentNullException.ThrowIfNull(fallbackSort);

        var ordered = default(IOrderedQueryable<TSource>);
        var sortList = sorts
            .Where(s => !string.IsNullOrWhiteSpace(s.MemberName))
            .ToList();

        if (sortList.Count == 0)
        {
            sortList.Add(fallbackSort);
        }

        foreach (var sort in sortList)
        {
            var descending = sort.Direction == DynamicSortDirection.Desc;

            ordered = ordered is null
                ? source.OrderByTranslated<TSource, TDestination>(
                    sort.MemberName,
                    descending,
                    translator)
                : ordered.ThenByTranslated<TSource, TDestination>(
                    sort.MemberName,
                    descending,
                    translator);
        }

        return ordered!;
    }

    /// <summary>
    /// Applies dynamic filters, then dynamic sorting.
    /// </summary>
    public static IOrderedQueryable<TSource> ApplyDynamicQuery<TSource, TDestination>(
        this IQueryable<TSource> source,
        DynamicQueryOptions options,
        IExpressionTranslator translator,
        DynamicSort fallbackSort)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(translator);
        ArgumentNullException.ThrowIfNull(fallbackSort);

        return source
            .ApplyDynamicFilters<TSource, TDestination>(options.Filters, translator)
            .ApplyDynamicSorts<TSource, TDestination>(options.Sorts, translator, fallbackSort);
    }

    private static object RequireValue(DynamicFilter filter)
    {
        return filter.Value
               ?? throw new ArgumentException(
                   $"Filter '{filter.MemberName}' with operator '{filter.Operator}' requires a non-null value.");
    }

    private static string RequireStringValue(DynamicFilter filter)
    {
        return filter.Value as string
               ?? throw new ArgumentException(
                   $"Filter '{filter.MemberName}' with operator '{filter.Operator}' requires a string value.");
    }
}