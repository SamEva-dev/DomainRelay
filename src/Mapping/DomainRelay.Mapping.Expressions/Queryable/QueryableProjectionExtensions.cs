using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Projection;

namespace DomainRelay.Mapping.Expressions.Queryable;

public static class QueryableProjectionExtensions
{
    public static IQueryable<TDestination> ProjectTo<TSource, TDestination>(
        this IQueryable<TSource> source,
        IProjectionBuilder projectionBuilder)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(projectionBuilder);

        var projection = projectionBuilder.BuildProjection<TSource, TDestination>();
        return System.Linq.Queryable.Select(source, projection);
    }

    public static IQueryable<TDestination> ProjectTo<TDestination>(
        this IQueryable source,
        IProjectionBuilder projectionBuilder)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(projectionBuilder);

        var projection = projectionBuilder.BuildProjection(source.ElementType, typeof(TDestination));

        return source.Provider.CreateQuery<TDestination>(
            Expression.Call(
                typeof(System.Linq.Queryable),
                nameof(System.Linq.Queryable.Select),
                new[] { source.ElementType, typeof(TDestination) },
                source.Expression,
                Expression.Quote(projection)));
    }
}