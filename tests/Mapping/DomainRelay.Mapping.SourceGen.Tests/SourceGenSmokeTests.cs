using System.Reflection;
using DomainRelay.Mapping.Abstractions.Generation;
using DomainRelay.Mapping.SourceGen;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace DomainRelay.Mapping.SourceGen.Tests;

public sealed class SourceGenSmokeTests
{
    [Fact]
    public void Generator_Should_Emit_Mapping_And_Registry_For_Supported_Types()
    {
        // Arrange
        const string source = """
using System;
using DomainRelay.Mapping.Abstractions.Generation;

namespace Demo;

[GenerateMapping(typeof(User), typeof(UserDto))]
public sealed partial class MappingMarker
{
}

public sealed class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

public sealed class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
""";

        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var compilation = CreateCompilation(
            syntaxTree,
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Guid).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(GenerateMappingAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IGeneratedMappingRegistry).Assembly.Location),
            });

        var generator = new DomainRelayMappingIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();

        // Assert - diagnostics
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Should()
            .BeEmpty();

        runResult.Results.Should().HaveCount(1);

        var generatedSources = runResult.Results[0].GeneratedSources;
        generatedSources.Should().HaveCount(2);

        var generatedTexts = generatedSources
            .Select(x => x.SourceText.ToString())
            .ToArray();

        generatedTexts.Should().Contain(x => x.Contains("internal static class GeneratedMappings", StringComparison.Ordinal));
        generatedTexts.Should().Contain(x => x.Contains("internal sealed class GeneratedMappingRegistry", StringComparison.Ordinal));
        generatedTexts.Should().Contain(x => x.Contains("Map_User_To_UserDto", StringComparison.Ordinal));
        generatedTexts.Should().Contain(x => x.Contains("Id = source.Id", StringComparison.Ordinal));
        generatedTexts.Should().Contain(x => x.Contains("Name = source.Name", StringComparison.Ordinal));
        generatedTexts.Should().Contain(x => x.Contains("Age = source.Age", StringComparison.Ordinal));
    }

    [Fact]
    public void Generator_Should_Not_Emit_When_No_Supported_Assignments_Exist()
    {
        // Arrange
        const string source = """
using System;
using DomainRelay.Mapping.Abstractions.Generation;

namespace Demo;

[GenerateMapping(typeof(SourceModel), typeof(DestinationModel))]
public sealed partial class MappingMarker
{
}

public sealed class SourceModel
{
    public Guid Id { get; set; }
    public string Age { get; set; } = string.Empty;
}

public sealed class DestinationModel
{
    public int Age { get; set; }
}
""";

        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var compilation = CreateCompilation(
            syntaxTree,
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Guid).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(GenerateMappingAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IGeneratedMappingRegistry).Assembly.Location),
            });

        var generator = new DomainRelayMappingIncrementalGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Act
        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        var runResult = driver.GetRunResult();

        // Assert
        diagnostics.Should().BeEmpty();
        outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Should()
            .BeEmpty();

        runResult.Results.Should().HaveCount(1);
        runResult.Results[0].GeneratedSources.Should().BeEmpty();
    }

    private static CSharpCompilation CreateCompilation(
        SyntaxTree syntaxTree,
        IEnumerable<MetadataReference> references)
    {
        return CSharpCompilation.Create(
            assemblyName: $"DomainRelay.Mapping.SourceGen.Tests_{Guid.NewGuid():N}",
            syntaxTrees: new[] { syntaxTree },
            references: references.Concat(GetFrameworkReferences()),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static IEnumerable<MetadataReference> GetFrameworkReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (!string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
        {
            return trustedPlatformAssemblies
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(static p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(static p => MetadataReference.CreateFromFile(p));
        }

        var assemblies = new[]
        {
            typeof(object).Assembly,
            typeof(Attribute).Assembly,
            typeof(Console).Assembly,
            typeof(Enumerable).Assembly,
            typeof(List<>).Assembly,
            typeof(System.Runtime.GCSettings).Assembly,
        };

        return assemblies
            .Select(a => a.Location)
            .Where(static p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(static p => MetadataReference.CreateFromFile(p));
    }
}
