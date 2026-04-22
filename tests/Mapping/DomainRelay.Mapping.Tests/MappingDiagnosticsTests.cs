using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Cache;
using DomainRelay.Mapping.Collections;
using DomainRelay.Mapping.Configuration;
using DomainRelay.Mapping.Diagnostics;
using DomainRelay.Mapping.Engine;
using DomainRelay.Mapping.Planning;
using DomainRelay.Mapping.Resolution;
using DomainRelay.Mapping.Resolution.Converters;
using DomainRelay.Mapping.Validation;
using FluentAssertions;

namespace DomainRelay.Mapping.Tests;

public sealed class MappingDiagnosticsTests
{
    [Fact]
    public void AssertConfigurationIsValid_Should_Return_Detailed_Message()
    {
        IMappingConfiguration configuration = new MappingConfiguration();
        configuration.CreateMap<InvalidSource, InvalidDestination>();

        var act = () => configuration.AssertConfigurationIsValid();

        var ex = act.Should().Throw<MappingValidationException>().Which;
        ex.Message.Should().Contain("Mapping configuration is invalid");
        ex.Message.Should().Contain(nameof(InvalidDestination.RequiredCount));
    }

    [Fact]
    public void Map_Should_Throw_Detailed_Execution_Exception_When_Strict_Mode_Is_Enabled()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<SourceWithTextNumber, DestinationWithNumber>();

        var mapper = CreateMapper(
            configuration,
            new DomainRelay.Mapping.Abstractions.Configuration.MappingRuntimeOptions
            {
                ThrowOnMappingFailure = true
            });

        var act = () => mapper.Map<SourceWithTextNumber, DestinationWithNumber>(
            new SourceWithTextNumber { Count = "not-a-number" });

        var ex = act.Should().Throw<MappingExecutionException>().Which;
        ex.Message.Should().Contain(typeof(SourceWithTextNumber).FullName!);
        ex.Message.Should().Contain(typeof(DestinationWithNumber).FullName!);
    }

    [Fact]
    public void Map_Should_Collect_Diagnostic_On_Execution_Error()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<SourceWithTextNumber, DestinationWithNumber>();

        var collector = new InMemoryMappingDiagnosticsCollector();

        var mapper = new ObjectMapper(
            configuration,
            new TypeMapFactory(configuration),
            new MappingPlanBuilder(),
            new MappingPlanCache(),
            new CompiledMappingPlanCache(),
            new MappingValidator(),
            CreateConverterRegistry(),
            new CollectionMapper(),
            new DictionaryMapper(),
            new DomainRelay.Mapping.Abstractions.Configuration.MappingRuntimeOptions
            {
                EnableDiagnostics = true,
                ThrowOnMappingFailure = false
            },
            collector);

        var result = mapper.Map<SourceWithTextNumber, DestinationWithNumber>(
            new SourceWithTextNumber { Count = "not-a-number" });

        result.Should().BeNull();
        collector.Items.Should().Contain(x =>
            x.Category == "ExecutionError" &&
            x.Message.Contains("Failed to map object.", StringComparison.Ordinal));
    }

    private static ObjectMapper CreateMapper(
        MappingConfiguration configuration,
        DomainRelay.Mapping.Abstractions.Configuration.MappingRuntimeOptions runtimeOptions)
    {
        return new ObjectMapper(
            configuration,
            new TypeMapFactory(configuration),
            new MappingPlanBuilder(),
            new MappingPlanCache(),
            new CompiledMappingPlanCache(),
            new MappingValidator(),
            CreateConverterRegistry(),
            new CollectionMapper(),
            new DictionaryMapper(),
            runtimeOptions,
            new InMemoryMappingDiagnosticsCollector());
    }

    private static TypeConverterRegistry CreateConverterRegistry()
    {
        var registry = new TypeConverterRegistry();
        registry.Register(new ToStringTypeConverter());
        registry.Register(new NullableTypeConverter());
        registry.Register(new EnumByNameTypeConverter());
        registry.Register(new EnumToStringTypeConverter());
        registry.Register(new EnumToEnumTypeConverter());
        registry.Register(new NumberToEnumTypeConverter());
        return registry;
    }

    public sealed class InvalidSource
    {
        public string? Name { get; set; }
    }

    public sealed class InvalidDestination
    {
        public int RequiredCount { get; set; }
    }

    public sealed class SourceWithTextNumber
    {
        public string? Count { get; set; }
    }

    public sealed class DestinationWithNumber
    {
        public int Count { get; set; }
    }
}