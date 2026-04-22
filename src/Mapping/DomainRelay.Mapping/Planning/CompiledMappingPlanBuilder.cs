using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Engine;

namespace DomainRelay.Mapping.Planning;

internal sealed class CompiledMappingPlanBuilder
{
    public CompiledMappingPlan Build(TypeMap typeMap)
    {
        var method = typeof(CompiledMappingPlanBuilder)
            .GetMethod(nameof(BuildTyped), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .MakeGenericMethod(typeMap.SourceType, typeMap.DestinationType);

        return (CompiledMappingPlan)method.Invoke(this, new object[] { typeMap })!;
    }

    private CompiledMappingPlan BuildTyped<TSource, TDestination>(TypeMap typeMap)
    {
        var sourceParam = Expression.Parameter(typeof(object), "source");
        var destinationParam = Expression.Parameter(typeof(object), "destination");
        var contextParam = Expression.Parameter(typeof(MappingContext), "context");

        var typedSource = Expression.Variable(typeof(TSource), "typedSource");
        var typedDestination = Expression.Variable(typeof(TDestination), "typedDestination");

        var expressions = new List<Expression>
        {
            Expression.Assign(typedSource, Expression.Convert(sourceParam, typeof(TSource)))
        };

        Expression createDestinationExpression;
        if (typeMap.ConstructionFactory is not null)
        {
            createDestinationExpression = Expression.Convert(
                Expression.Invoke(Expression.Constant(typeMap.ConstructionFactory), Expression.Convert(typedSource, typeof(object))),
                typeof(TDestination));
        }
        else
        {
            var ctor = typeof(TDestination).GetConstructor(Type.EmptyTypes);
            if (ctor is null)
            {
                throw new InvalidOperationException(
                    $"No parameterless constructor available for '{typeof(TDestination).FullName}' and no construction factory configured.");
            }

            createDestinationExpression = Expression.New(ctor);
        }

        expressions.Add(
            Expression.Assign(
                typedDestination,
                Expression.Condition(
                    Expression.NotEqual(destinationParam, Expression.Constant(null)),
                    Expression.Convert(destinationParam, typeof(TDestination)),
                    createDestinationExpression)));

        foreach (var beforeAction in typeMap.BeforeMapActions)
        {
            expressions.Add(
                Expression.Invoke(
                    Expression.Constant(beforeAction),
                    Expression.Convert(typedSource, typeof(object)),
                    Expression.Convert(typedDestination, typeof(object))));
        }

        foreach (var memberMap in typeMap.MemberMaps.Where(m => !m.Ignored))
        {
            expressions.Add(BuildMemberAssignmentExpression(memberMap, typedSource, typedDestination));
        }

        foreach (var afterAction in typeMap.AfterMapActions)
        {
            expressions.Add(
                Expression.Invoke(
                    Expression.Constant(afterAction),
                    Expression.Convert(typedSource, typeof(object)),
                    Expression.Convert(typedDestination, typeof(object))));
        }

        expressions.Add(Expression.Convert(typedDestination, typeof(object)));

        var body = Expression.Block(
            new[] { typedSource, typedDestination },
            expressions);

        var lambda = Expression.Lambda<CompiledMappingDelegate>(
            body,
            sourceParam,
            destinationParam,
            contextParam);

        return new CompiledMappingPlan(
            typeof(TSource),
            typeof(TDestination),
            lambda.Compile());
    }

    private static Expression BuildMemberAssignmentExpression(
        MemberMap memberMap,
        ParameterExpression typedSource,
        ParameterExpression typedDestination)
    {
        var destinationProperty = Expression.Property(typedDestination, memberMap.DestinationProperty);

        var resolverInvoke = Expression.Invoke(
            Expression.Constant(memberMap.ValueResolver!),
            Expression.Convert(typedSource, typeof(object)));

        Expression resolvedValue = resolverInvoke;

        if (memberMap.NullSubstitute is not null)
        {
            resolvedValue = Expression.Condition(
                Expression.Equal(resolvedValue, Expression.Constant(null)),
                Expression.Constant(memberMap.NullSubstitute),
                resolvedValue);
        }

        var convertedValue = Expression.Convert(resolvedValue, memberMap.DestinationProperty.PropertyType);
        Expression assignment = Expression.Assign(destinationProperty, convertedValue);

        if (memberMap.PreCondition is not null)
        {
            var preConditionInvoke = Expression.Invoke(
                Expression.Constant(memberMap.PreCondition),
                Expression.Convert(typedSource, typeof(object)));

            assignment = Expression.IfThen(preConditionInvoke, assignment);
        }

        if (memberMap.Condition is not null)
        {
            var conditionInvoke = Expression.Invoke(
                Expression.Constant(memberMap.Condition),
                Expression.Convert(typedSource, typeof(object)),
                Expression.Convert(typedDestination, typeof(object)));

            assignment = Expression.IfThen(conditionInvoke, assignment);
        }

        return assignment;
    }
}
