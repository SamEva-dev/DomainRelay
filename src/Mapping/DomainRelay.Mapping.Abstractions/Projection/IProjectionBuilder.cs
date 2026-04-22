using System.Linq.Expressions;

namespace DomainRelay.Mapping.Abstractions.Projection;

public interface IProjectionBuilder
{
    Expression<Func<TSource, TDestination>> BuildProjection<TSource, TDestination>();

    LambdaExpression BuildProjection(Type sourceType, Type destinationType);
}