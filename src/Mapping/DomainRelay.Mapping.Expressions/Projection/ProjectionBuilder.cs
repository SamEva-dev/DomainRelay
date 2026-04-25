using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Exceptions;
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
        return (Expression<Func<TSource, TDestination>>)BuildProjection(
            typeof(TSource),
            typeof(TDestination));
    }

    public LambdaExpression BuildProjection(Type sourceType, Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(sourceType);
        ArgumentNullException.ThrowIfNull(destinationType);

        var plan = _planBuilder.Build(sourceType, destinationType);
        _validator.Validate(plan);

        try
        {
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

                body = EnsureAssignableExpression(
                    body,
                    member.DestinationProperty.PropertyType,
                    sourceType,
                    destinationType,
                    member.DestinationMemberName);

                if (plan.Constructor is null || !IsConstructorParameter(plan, member.DestinationMemberName))
                {
                    bindings.Add(Expression.Bind(member.DestinationProperty, body));
                }
            }

            Expression bodyExpression;

            if (plan.Constructor is not null)
            {
                var parameters = plan.Constructor.GetParameters();

                foreach (var parameter in parameters)
                {
                    var member = plan.Members.First(m =>
                        string.Equals(m.DestinationMemberName, parameter.Name, StringComparison.OrdinalIgnoreCase));

                    var sourceBody = member.SourceExpressionBody
                                     ?? throw new ProjectionConfigurationException(
                                         sourceType,
                                         destinationType,
                                         $"Constructor parameter '{parameter.Name}' has no source expression.");

                    var arg = ParameterReplaceVisitor.Replace(
                        sourceBody,
                        ExpressionParameterFinder.FindRootParameter(sourceBody),
                        sourceParameter);

                    arg = EnsureAssignableExpression(
                        arg,
                        parameter.ParameterType,
                        sourceType,
                        destinationType,
                        parameter.Name ?? member.DestinationMemberName);

                    ctorArgs.Add(arg);
                }

                var newExpression = Expression.New(plan.Constructor, ctorArgs);

                bodyExpression = bindings.Count > 0
                    ? Expression.MemberInit(newExpression, bindings)
                    : newExpression;
            }
            else
            {
                bodyExpression = Expression.MemberInit(Expression.New(destinationType), bindings);
            }

            var delegateType = typeof(Func<,>).MakeGenericType(sourceType, destinationType);

            return Expression.Lambda(delegateType, bodyExpression, sourceParameter);
        }
        catch (ProjectionConfigurationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ProjectionConfigurationException(
                sourceType,
                destinationType,
                $"Unable to build projection expression. Inner: {ex.Message}");
        }
    }

    private static bool IsConstructorParameter(ProjectionPlan plan, string destinationMemberName)
    {
        if (plan.Constructor is null)
            return false;

        return plan.Constructor
            .GetParameters()
            .Any(p => string.Equals(
                p.Name,
                destinationMemberName,
                StringComparison.OrdinalIgnoreCase));
    }

    private static Expression EnsureAssignableExpression(
        Expression expression,
        Type destinationType,
        Type sourceRootType,
        Type destinationRootType,
        string? memberName)
    {
        if (destinationType.IsAssignableFrom(expression.Type))
        {
            return expression;
        }

        if (CanUseExpressionConvert(expression.Type, destinationType))
        {
            return Expression.Convert(expression, destinationType);
        }

        throw new ProjectionConfigurationException(
            sourceRootType,
            destinationRootType,
            $"Member '{memberName}' cannot be projected from '{expression.Type.FullName}' to '{destinationType.FullName}'.");
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

        return IsNumericType(actualSource) && IsNumericType(actualDestination);
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(byte)
               || type == typeof(sbyte)
               || type == typeof(short)
               || type == typeof(ushort)
               || type == typeof(int)
               || type == typeof(uint)
               || type == typeof(long)
               || type == typeof(ulong)
               || type == typeof(float)
               || type == typeof(double)
               || type == typeof(decimal);
    }
}