using DomainRelay.Mapping.SourceGen.Models;
using Microsoft.CodeAnalysis;

namespace DomainRelay.Mapping.SourceGen.Analysis;

internal static class MappingCapabilityAnalyzer
{
    public static bool TryBuildModel(
        INamedTypeSymbol source,
        INamedTypeSymbol destination,
        out GeneratedMappingModel? model)
    {
        var destinationProperties = destination
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic && p.SetMethod is not null)
            .ToArray();

        var sourceProperties = source
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic && p.GetMethod is not null)
            .ToDictionary(p => p.Name, p => p, StringComparer.Ordinal);

        var assignments = new List<GeneratedMemberAssignmentModel>();

        foreach (var destinationProperty in destinationProperties)
        {
            if (!sourceProperties.TryGetValue(destinationProperty.Name, out var sourceProperty))
            {
                continue;
            }

            if (!SymbolEqualityComparer.Default.Equals(sourceProperty.Type, destinationProperty.Type))
            {
                continue;
            }

            assignments.Add(new GeneratedMemberAssignmentModel
            {
                DestinationMemberName = destinationProperty.Name,
                SourceAccessCode = $"source.{sourceProperty.Name}",
                Ignored = false,
                NullSubstituteCode = null,
                RequiresNestedMapping = false
            });
        }

        model = new GeneratedMappingModel
        {
            SourceTypeDisplayName = source.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            DestinationTypeDisplayName = destination.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            MappingMethodName = BuildMethodName(source, destination),
            BoxedMethodName = BuildBoxedMethodName(source, destination),
            Members = assignments
        };

        return assignments.Count > 0;
    }

    private static string BuildMethodName(INamedTypeSymbol source, INamedTypeSymbol destination)
    {
        return $"Map_{Sanitize(source.Name)}_To_{Sanitize(destination.Name)}";
    }

    private static string BuildBoxedMethodName(INamedTypeSymbol source, INamedTypeSymbol destination)
    {
        return $"Map_{Sanitize(source.Name)}_To_{Sanitize(destination.Name)}_Boxed";
    }

    private static string Sanitize(string name)
    {
        return name.Replace("<", "_").Replace(">", "_").Replace(",", "_");
    }
}