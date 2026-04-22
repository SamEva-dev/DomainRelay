namespace DomainRelay.Mapping.Expressions.Projection;

internal sealed class ProjectionValidator
{
    public void Validate(ProjectionPlan plan)
    {
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
                    $"Projection member '{plan.DestinationType.FullName}.{member.DestinationMemberName}' has no projectable expression.");
                continue;
            }

            var destinationType = member.DestinationProperty.PropertyType;
            var sourceExpressionType = member.SourceExpressionBody.Type;

            if (!destinationType.IsAssignableFrom(sourceExpressionType) &&
                !CanUseExpressionConvert(sourceExpressionType, destinationType))
            {
                errors.Add(
                    $"Projection member '{plan.DestinationType.FullName}.{member.DestinationMemberName}' cannot be projected from '{sourceExpressionType.FullName}' to '{destinationType.FullName}'.");
            }
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        }
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