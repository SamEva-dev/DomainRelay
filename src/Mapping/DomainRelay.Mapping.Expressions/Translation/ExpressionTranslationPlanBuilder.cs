using System.Linq.Expressions;
using System.Reflection;
using DomainRelay.Mapping.Configuration;
using DomainRelay.Mapping.Expressions.Projection;

namespace DomainRelay.Mapping.Expressions.Translation;

internal sealed class ExpressionTranslationPlanBuilder
{
    private readonly MappingConfiguration _configuration;

    public ExpressionTranslationPlanBuilder(MappingConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ExpressionTranslationPlan Build(Type sourceType, Type destinationType)
    {
        _configuration.TryGetMap(sourceType, destinationType, out var mapExpressionObject);

        var sourceParameter = Expression.Parameter(sourceType, "src");
        var members = new Dictionary<string, DestinationMemberTranslationMap>(StringComparer.OrdinalIgnoreCase);

        var destinationMembers = destinationType
            .GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.MemberType is MemberTypes.Property or MemberTypes.Field)
            .ToArray();

        if (mapExpressionObject is not null && !sourceType.IsGenericTypeDefinition && !destinationType.IsGenericTypeDefinition)
        {
            var typedMethod = typeof(ExpressionTranslationPlanBuilder)
                .GetMethod(nameof(BuildFromTypedMap), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(sourceType, destinationType);

            return (ExpressionTranslationPlan)typedMethod.Invoke(this, new[] { mapExpressionObject })!;
        }

        foreach (var member in destinationMembers)
        {
            var sourceProperty = sourceType.GetProperty(member.Name, BindingFlags.Public | BindingFlags.Instance);
            if (sourceProperty is not null && sourceProperty.CanRead)
            {
                members[member.Name] = new DestinationMemberTranslationMap(
                    member.Name,
                    member,
                    Expression.Property(sourceParameter, sourceProperty));
                continue;
            }

            var flattening = ProjectionFlatteningResolver.TryBuildExpression(sourceType, member.Name, sourceParameter);
            if (flattening is not null)
            {
                members[member.Name] = new DestinationMemberTranslationMap(
                    member.Name,
                    member,
                    flattening);
            }
        }

        return new ExpressionTranslationPlan(sourceType, destinationType, members);
    }

    private ExpressionTranslationPlan BuildFromTypedMap<TSource, TDestination>(
        MapExpression<TSource, TDestination> expression)
    {
        var sourceParameter = Expression.Parameter(typeof(TSource), "src");
        var members = new Dictionary<string, DestinationMemberTranslationMap>(StringComparer.OrdinalIgnoreCase);

        var destinationMembers = typeof(TDestination)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.MemberType is MemberTypes.Property or MemberTypes.Field)
            .ToArray();

        foreach (var member in destinationMembers)
        {
            if (expression.MemberMaps.TryGetValue(member.Name, out var explicitMember))
            {
                if (explicitMember.Ignored)
                {
                    continue;
                }

                Expression? body = null;

                if (explicitMember.SourceExpression is not null)
                {
                    body = ParameterReplaceVisitor.Replace(
                        explicitMember.SourceExpression.Body,
                        explicitMember.SourceExpression.Parameters[0],
                        sourceParameter);
                }
                else
                {
                    var sourceProperty = typeof(TSource).GetProperty(member.Name);
                    if (sourceProperty is not null)
                    {
                        body = Expression.Property(sourceParameter, sourceProperty);
                    }
                }

                if (body is not null)
                {
                    members[member.Name] = new DestinationMemberTranslationMap(
                        member.Name,
                        member,
                        body);
                }

                continue;
            }

            var directSourceProperty = typeof(TSource).GetProperty(member.Name);
            if (directSourceProperty is not null && directSourceProperty.CanRead)
            {
                members[member.Name] = new DestinationMemberTranslationMap(
                    member.Name,
                    member,
                    Expression.Property(sourceParameter, directSourceProperty));
                continue;
            }

            var flattening = ProjectionFlatteningResolver.TryBuildExpression(typeof(TSource), member.Name, sourceParameter);
            if (flattening is not null)
            {
                members[member.Name] = new DestinationMemberTranslationMap(
                    member.Name,
                    member,
                    flattening);
            }
        }

        return new ExpressionTranslationPlan(typeof(TSource), typeof(TDestination), members);
    }
}