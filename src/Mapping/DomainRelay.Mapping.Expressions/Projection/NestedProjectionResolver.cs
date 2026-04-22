using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.Internal;
using DomainRelay.Mapping.Expressions.Translation;

namespace DomainRelay.Mapping.Expressions.Projection;

internal sealed class NestedProjectionResolver
{
    private readonly IProjectionBuilder _projectionBuilder;

    public NestedProjectionResolver(IProjectionBuilder projectionBuilder)
    {
        _projectionBuilder = projectionBuilder;
    }

    public Expression? TryBuildNestedProjection(Expression sourceExpression, Type sourceType, Type destinationType)
    {
        if (TypeHelper.IsSimpleType(sourceType) || TypeHelper.IsSimpleType(destinationType))
        {
            return null;
        }

        var projection = _projectionBuilder.BuildProjection(sourceType, destinationType);

        return ParameterReplaceVisitor.Replace(
            projection.Body,
            projection.Parameters[0],
            sourceExpression);
    }
}