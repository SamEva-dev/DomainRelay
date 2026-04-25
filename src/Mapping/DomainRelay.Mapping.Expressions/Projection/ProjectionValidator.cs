using DomainRelay.Mapping.Abstractions.Exceptions;

namespace DomainRelay.Mapping.Expressions.Projection;

internal sealed class ProjectionValidator
{
    public void Validate(ProjectionPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var errors = new List<string>();

        foreach (var member in plan.Members)
        {
            if (member.Ignored)
            {
                continue;
            }

            if (member.SourceExpressionBody is null)
            {
                errors.Add(
                    $"Member '{plan.DestinationType.FullName}.{member.DestinationMemberName}' has no projectable source expression.");
                continue;
            }

            var destinationType = member.DestinationProperty.PropertyType;
            var sourceExpressionType = member.SourceExpressionBody.Type;

            if (!destinationType.IsAssignableFrom(sourceExpressionType) &&
                !CanUseExpressionConvert(sourceExpressionType, destinationType))
            {
                errors.Add(
                    $"Member '{plan.DestinationType.FullName}.{member.DestinationMemberName}' cannot be projected from '{sourceExpressionType.FullName}' to '{destinationType.FullName}'.");
            }
        }

        if (plan.Constructor is null && !HasPublicParameterlessConstructor(plan.DestinationType))
        {
            errors.Add(
                $"Destination type '{plan.DestinationType.FullName}' has no public parameterless constructor and no resolvable constructor for projection.");
        }

        if (plan.Constructor is not null)
        {
            foreach (var parameter in plan.Constructor.GetParameters())
            {
                var member = plan.Members.FirstOrDefault(m =>
                    string.Equals(m.DestinationMemberName, parameter.Name, StringComparison.OrdinalIgnoreCase));

                if (member?.SourceExpressionBody is null)
                {
                    errors.Add(
                        $"Constructor parameter '{parameter.Name}' on '{plan.DestinationType.FullName}' has no projectable source expression.");
                    continue;
                }

                if (!parameter.ParameterType.IsAssignableFrom(member.SourceExpressionBody.Type) &&
                    !CanUseExpressionConvert(member.SourceExpressionBody.Type, parameter.ParameterType))
                {
                    errors.Add(
                        $"Constructor parameter '{parameter.Name}' on '{plan.DestinationType.FullName}' cannot be projected from '{member.SourceExpressionBody.Type.FullName}' to '{parameter.ParameterType.FullName}'.");
                }
            }
        }

        if (errors.Count > 0)
        {
            throw new ProjectionConfigurationException(
                plan.SourceType,
                plan.DestinationType,
                errors);
        }
    }

    private static bool HasPublicParameterlessConstructor(Type type)
    {
        return type.GetConstructor(Type.EmptyTypes) is not null;
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