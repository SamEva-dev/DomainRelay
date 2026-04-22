using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.Expressions.Translation;

namespace DomainRelay.Mapping.Expressions.Projection;

internal sealed class ProjectionBuilder : IProjectionBuilder
{
    private readonly ProjectionPlanBuilder _planBuilder;
    private readonly ProjectionValidator _validator;

    public ProjectionBuilder(ProjectionPlanBuilder planBuilder, ProjectionValidator validator)
    {
        _planBuilder = planBuilder;
        _validator = validator;
    }

    public Expression<Func<TSource, TDestination>> BuildProjection<TSource, TDestination>()
    {
        return (Expression<Func<TSource, TDestination>>)BuildProjection(typeof(TSource), typeof(TDestination));
    }

    public LambdaExpression BuildProjection(Type sourceType, Type destinationType)
    {
        var plan = _planBuilder.Build(sourceType, destinationType);
        _validator.Validate(plan);

        var sourceParameter = Expression.Parameter(sourceType, "src");
        var bindings = new List<MemberBinding>();
        var ctorArgs = new List<Expression>();

        foreach (var member in plan.Members)
        {
            if (member.Ignored || member.SourceExpressionBody is null)
            {
                continue;
            }

            var body = ParameterReplaceVisitor.Replace(
                member.SourceExpressionBody,
                ExpressionParameterFinder.FindRootParameter(member.SourceExpressionBody),
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
                        $"Cannot build projection constructor argument '{parameter.Name}' for '{destinationType.FullName}'.");
                }

                Expression arg = ParameterReplaceVisitor.Replace(
                    member.SourceExpressionBody,
                    ExpressionParameterFinder.FindRootParameter(member.SourceExpressionBody),
                    sourceParameter);
                if (!parameter.ParameterType.IsAssignableFrom(arg.Type))
                {
                    arg = Expression.Convert(arg, parameter.ParameterType);
                }

                ctorArgs.Add(arg);
            }

            var newExpression = Expression.New(plan.Constructor, ctorArgs);

            if (bindings.Count > 0 && destinationType.GetProperties().Any(p => p.CanWrite))
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
            bodyExpression = Expression.MemberInit(Expression.New(destinationType), bindings);
        }

        var delegateType = typeof(Func<,>).MakeGenericType(sourceType, destinationType);
        return Expression.Lambda(delegateType, bodyExpression, sourceParameter);
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
}
