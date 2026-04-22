using System.Linq.Expressions;
using System.Reflection;
using DomainRelay.Mapping.Configuration;
using DomainRelay.Mapping.Expressions.Translation;

namespace DomainRelay.Mapping.Expressions.Projection;

internal sealed class ProjectionPlanBuilder
{
    private readonly MappingConfiguration _configuration;

    public ProjectionPlanBuilder(MappingConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ProjectionPlan Build(Type sourceType, Type destinationType)
    {
        _configuration.TryGetMap(sourceType, destinationType, out var mapExpressionObject);

        var destinationProperties = destinationType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToArray();

        var members = new List<ProjectionMemberMap>();

        if (mapExpressionObject is not null && !sourceType.IsGenericTypeDefinition && !destinationType.IsGenericTypeDefinition)
        {
            var typedMethod = typeof(ProjectionPlanBuilder)
                .GetMethod(nameof(BuildFromTypedMap), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(sourceType, destinationType);

            return (ProjectionPlan)typedMethod.Invoke(this, new[] { mapExpressionObject })!;
        }

        var sourceParameter = Expression.Parameter(sourceType, "src");

        foreach (var destinationProperty in destinationProperties)
        {
            var sourceProperty = sourceType.GetProperty(destinationProperty.Name, BindingFlags.Public | BindingFlags.Instance);
            if (sourceProperty is not null && sourceProperty.CanRead)
            {
                var directBody = BuildDirectOrNestedProjectionBody(
                    sourceParameter,
                    sourceProperty,
                    destinationProperty.PropertyType);

                if (directBody is not null)
                {
                    members.Add(new ProjectionMemberMap(
                        destinationProperty.Name,
                        destinationProperty,
                        directBody,
                        ignored: false,
                        isExplicit: false,
                        nullSubstitute: null));
                    continue;
                }
            }

            var flattening = ProjectionFlatteningResolver.TryBuildExpression(sourceType, destinationProperty.Name, sourceParameter);
            if (flattening is not null)
            {
                members.Add(new ProjectionMemberMap(
                    destinationProperty.Name,
                    destinationProperty,
                    flattening,
                    ignored: false,
                    isExplicit: false,
                    nullSubstitute: null));
            }
        }

        var constructor = ProjectionConstructorResolver.TryResolve(destinationType, members);
        return new ProjectionPlan(sourceType, destinationType, members, constructor);
    }

    private ProjectionPlan BuildFromTypedMap<TSource, TDestination>(
        MapExpression<TSource, TDestination> expression)
    {
        var destinationProperties = typeof(TDestination)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToArray();

        var sourceParameter = Expression.Parameter(typeof(TSource), "src");
        var members = new List<ProjectionMemberMap>();

        foreach (var destinationProperty in destinationProperties)
        {
            if (expression.MemberMaps.TryGetValue(destinationProperty.Name, out var explicitMember))
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
                    var sourceProperty = typeof(TSource).GetProperty(destinationProperty.Name);
                    if (sourceProperty is not null)
                    {
                        body = BuildDirectOrNestedProjectionBody(
                            sourceParameter,
                            sourceProperty,
                            destinationProperty.PropertyType);
                    }
                }

                if (body is not null && explicitMember.NullSubstitute is not null)
                {
                    if (!body.Type.IsValueType || Nullable.GetUnderlyingType(body.Type) is not null)
                    {
                        body = Expression.Coalesce(
                            body,
                            Expression.Constant(explicitMember.NullSubstitute, body.Type));
                    }
                }

                members.Add(new ProjectionMemberMap(
                    destinationProperty.Name,
                    destinationProperty,
                    body,
                    ignored: false,
                    isExplicit: true,
                    explicitMember.NullSubstitute));

                continue;
            }

            var directSourceProperty = typeof(TSource).GetProperty(destinationProperty.Name);
            if (directSourceProperty is not null && directSourceProperty.CanRead)
            {
                var directBody = BuildDirectOrNestedProjectionBody(
                    sourceParameter,
                    directSourceProperty,
                    destinationProperty.PropertyType);

                if (directBody is not null)
                {
                    members.Add(new ProjectionMemberMap(
                        destinationProperty.Name,
                        destinationProperty,
                        directBody,
                        ignored: false,
                        isExplicit: false,
                        nullSubstitute: null));
                    continue;
                }
            }

            var flattening = ProjectionFlatteningResolver.TryBuildExpression(typeof(TSource), destinationProperty.Name, sourceParameter);
            if (flattening is not null)
            {
                members.Add(new ProjectionMemberMap(
                    destinationProperty.Name,
                    destinationProperty,
                    flattening,
                    ignored: false,
                    isExplicit: false,
                    nullSubstitute: null));
            }
        }

        var constructor = ProjectionConstructorResolver.TryResolve(typeof(TDestination), members);
        return new ProjectionPlan(typeof(TSource), typeof(TDestination), members, constructor);
    }

    private Expression? BuildDirectOrNestedProjectionBody(
        Expression sourceRoot,
        PropertyInfo sourceProperty,
        Type destinationMemberType)
    {
        var directAccess = Expression.Property(sourceRoot, sourceProperty);

        if (destinationMemberType.IsAssignableFrom(sourceProperty.PropertyType))
        {
            return directAccess;
        }

        if (!_configuration.TryGetMap(sourceProperty.PropertyType, destinationMemberType, out _))
        {
            return null;
        }

        var nestedPlan = Build(sourceProperty.PropertyType, destinationMemberType);
        var nestedLambda = BuildNestedLambda(nestedPlan);
        return ParameterReplaceVisitor.Replace(
            nestedLambda.Body,
            nestedLambda.Parameters[0],
            directAccess);
    }

    private static LambdaExpression BuildNestedLambda(ProjectionPlan plan)
    {
        var sourceParameter = Expression.Parameter(plan.SourceType, "src");
        var bindings = new List<MemberBinding>();
        var ctorArgs = new List<Expression>();

        foreach (var member in plan.Members)
        {
            if (member.Ignored || member.SourceExpressionBody is null)
            {
                continue;
            }

            Expression body = ParameterReplaceVisitor.Replace(
                member.SourceExpressionBody,
                FindRootParameter(member.SourceExpressionBody),
                sourceParameter);

            if (!member.DestinationProperty.PropertyType.IsAssignableFrom(body.Type))
            {
                if (CanUseExpressionConvert(body.Type, member.DestinationProperty.PropertyType))
                {
                    body = Expression.Convert(body, member.DestinationProperty.PropertyType);
                }
                else
                {
                    continue;
                }
            }

            bindings.Add(Expression.Bind(member.DestinationProperty, body));
        }

        Expression bodyExpression;

        if (plan.Constructor is not null)
        {
            var parameters = plan.Constructor.GetParameters();
            foreach (var parameter in parameters)
            {
                var member = plan.Members.FirstOrDefault(m =>
                    string.Equals(m.DestinationMemberName, parameter.Name, StringComparison.OrdinalIgnoreCase));

                if (member?.SourceExpressionBody is null)
                {
                    throw new InvalidOperationException(
                        $"Cannot build nested projection constructor argument '{parameter.Name}' for '{plan.DestinationType.FullName}'.");
                }

                Expression arg = ParameterReplaceVisitor.Replace(
                    member.SourceExpressionBody,
                    FindRootParameter(member.SourceExpressionBody),
                    sourceParameter);

                if (!parameter.ParameterType.IsAssignableFrom(arg.Type))
                {
                    arg = Expression.Convert(arg, parameter.ParameterType);
                }

                ctorArgs.Add(arg);
            }

            var newExpression = Expression.New(plan.Constructor, ctorArgs);

            if (bindings.Count > 0 && plan.DestinationType.GetProperties().Any(p => p.CanWrite))
            {
                bodyExpression = Expression.MemberInit(newExpression, bindings);
            }
            else
            {
                bodyExpression = newExpression;
            }
        }
        else
        {
            bodyExpression = Expression.MemberInit(Expression.New(plan.DestinationType), bindings);
        }

        var delegateType = typeof(Func<,>).MakeGenericType(plan.SourceType, plan.DestinationType);
        return Expression.Lambda(delegateType, bodyExpression, sourceParameter);
    }

    private static ParameterExpression FindRootParameter(Expression expression)
    {
        var finder = new RootParameterFinder();
        finder.Visit(expression);
        return finder.Parameter
               ?? throw new InvalidOperationException("Unable to find root parameter in nested projection expression.");
    }

    private static bool CanUseExpressionConvert(Type sourceType, Type destinationType)
    {
        var actualSource = Nullable.GetUnderlyingType(sourceType) ?? sourceType;
        var actualDestination = Nullable.GetUnderlyingType(destinationType) ?? destinationType;

        if (actualDestination.IsAssignableFrom(actualSource))
        {
            return true;
        }

        if (actualSource.IsEnum && actualDestination == typeof(string))
        {
            return false;
        }

        if (actualSource == typeof(string) && actualDestination.IsEnum)
        {
            return false;
        }

        return actualSource.IsPrimitive && actualDestination.IsPrimitive;
    }

    private sealed class RootParameterFinder : ExpressionVisitor
    {
        public ParameterExpression? Parameter { get; private set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Parameter ??= node;
            return base.VisitParameter(node);
        }
    }
}