using DomainRelay.Mapping.SourceGen.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace DomainRelay.Mapping.SourceGen.Analysis;

internal static class AttributeMappingScanner
{
    public static IReadOnlyList<(INamedTypeSymbol Source, INamedTypeSymbol Destination)> Scan(
        Compilation compilation,
        IEnumerable<ClassDeclarationSyntax> candidates)
    {
        var results = new List<(INamedTypeSymbol Source, INamedTypeSymbol Destination)>();

        foreach (var candidate in candidates)
        {
            var model = compilation.GetSemanticModel(candidate.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(candidate);
            if (symbol is null)
            {
                continue;
            }

            foreach (var attribute in symbol.GetAttributes())
            {
                if (attribute.AttributeClass?.ToDisplayString() != "DomainRelay.Mapping.Abstractions.Generation.GenerateMappingAttribute")
                {
                    continue;
                }

                if (attribute.ConstructorArguments.Length != 2)
                {
                    continue;
                }

                if (attribute.ConstructorArguments[0].Value is INamedTypeSymbol source &&
                    attribute.ConstructorArguments[1].Value is INamedTypeSymbol destination)
                {
                    results.Add((source, destination));
                }
            }
        }

        return results;
    }
}