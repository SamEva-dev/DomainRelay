using DomainRelay.Mapping.SourceGen.Analysis;
using DomainRelay.Mapping.SourceGen.Emission;
using DomainRelay.Mapping.SourceGen.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace DomainRelay.Mapping.SourceGen;

[Generator]
public sealed class DomainRelayMappingIncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(static x => x is not null);

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationAndClasses, static (spc, pair) =>
        {
            var compilation = pair.Left;
            var classes = pair.Right;

            var discovered = AttributeMappingScanner.Scan(compilation, classes);
            var models = new List<GeneratedMappingModel>();

            foreach (var (source, destination) in discovered)
            {
                if (MappingCapabilityAnalyzer.TryBuildModel(source, destination, out var model) && model is not null)
                {
                    models.Add(model);
                }
            }

            if (models.Count == 0)
            {
                return;
            }

            spc.AddSource("GeneratedMappings.g.cs", MappingCodeEmitter.Emit(models));
            spc.AddSource("GeneratedMappingRegistry.g.cs", RegistryEmitter.Emit(models));
        });
    }
}