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
using DomainRelay.Mapping.Tests.Models;
using DomainRelay.Mapping.Validation;
using FluentAssertions;

namespace DomainRelay.Mapping.Tests;

public sealed class ObjectMapperCompiledPlanTests
{
    [Fact]
    public void Mapper_Should_Reuse_Compiled_Plan()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var compiledCache = new CompiledMappingPlanCache();
        var mapper = CreateMapper(configuration, compiledCache: compiledCache);

        mapper.Map<User, UserDto>(new User());
        mapper.Map<User, UserDto>(new User());

        compiledCache.TryGet(typeof(User), typeof(UserDto), out var cached).Should().BeTrue();
        cached.Should().NotBeNull();
        cached!.IsExecutable.Should().BeTrue();
    }

    [Fact]
    public void AssertConfigurationIsValid_Should_Not_Throw_For_Valid_Maps()
    {
        IMappingConfiguration configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var act = () => configuration.AssertConfigurationIsValid();

        act.Should().NotThrow();
    }

    [Fact]
    public void AssertConfigurationIsValid_Should_Throw_For_Invalid_Maps()
    {
        IMappingConfiguration configuration = new MappingConfiguration();
        configuration.CreateMap<BrokenSource, BrokenDestination>();

        var act = () => configuration.AssertConfigurationIsValid();

        act.Should().Throw<MappingValidationException>();
    }

    [Fact]
    public void Mapper_Should_Collect_Diagnostics_When_Enabled()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var collector = new InMemoryMappingDiagnosticsCollector();
        var mapper = CreateMapper(
            configuration,
            diagnosticsCollector: collector,
            runtimeOptions: new MappingRuntimeOptions { EnableDiagnostics = true });

        mapper.Map<User, UserDto>(new User());

        collector.Items.Should().NotBeEmpty();
        collector.Items.Any(x => x.Category == "CompiledPlan" && x.Message.Contains("Using compiled mapping plan.", StringComparison.Ordinal))
            .Should()
            .BeTrue();
    }

    [Fact]
    public void Mapper_Should_Fall_Back_To_Runtime_Plan_When_FastPath_Is_Disabled()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var collector = new InMemoryMappingDiagnosticsCollector();
        var compiledCache = new CompiledMappingPlanCache();
        var mapper = CreateMapper(
            configuration,
            compiledCache: compiledCache,
            diagnosticsCollector: collector,
            runtimeOptions: new MappingRuntimeOptions
            {
                EnableDiagnostics = true,
                EnableFastPathCompilation = false
            });

        var source = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var result = mapper.Map<User, UserDto>(source);

        result.FirstName.Should().Be("Sam");
        result.LastName.Should().Be("Fokam");

        compiledCache.TryGet(typeof(User), typeof(UserDto), out var cached).Should().BeTrue();
        cached.Should().NotBeNull();
        cached!.IsExecutable.Should().BeFalse();
        cached.FailureReason.Should().Be("Fast-path compilation is disabled.");

        collector.Items.Any(x => x.Category == "CompiledPlan" && x.Message.Contains("unavailable", StringComparison.OrdinalIgnoreCase))
            .Should()
            .BeTrue();
    }

    private static ObjectMapper CreateMapper(
        MappingConfiguration configuration,
        CompiledMappingPlanCache? compiledCache = null,
        IMappingDiagnosticsCollector? diagnosticsCollector = null,
        MappingRuntimeOptions? runtimeOptions = null)
    {
        return new ObjectMapper(
            configuration,
            new TypeMapFactory(configuration),
            new MappingPlanBuilder(),
            new MappingPlanCache(),
            compiledCache ?? new CompiledMappingPlanCache(),
            new MappingValidator(),
            CreateConverterRegistry(),
            new CollectionMapper(),
            new DictionaryMapper(),
            runtimeOptions ?? new MappingRuntimeOptions(),
            diagnosticsCollector ?? new InMemoryMappingDiagnosticsCollector());
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

    public sealed class BrokenSource
    {
        public string? Value { get; set; }
    }

    public sealed class BrokenDestination
    {
        public string Value { get; }

        public BrokenDestination(string value)
        {
            Value = value;
        }
    }
}