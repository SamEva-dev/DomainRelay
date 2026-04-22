using DomainRelay.Mapping.Abstractions.Resolvers;
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

public sealed class ObjectMapperResolversTests
{
    [Fact]
    public void Map_Should_Use_ResolveUsing_When_Configured()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonSource, PersonDestination>()
            .ForMember(d => d.DisplayName, o => o.ResolveUsing(new FullNameResolver()));

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<PersonSource, PersonDestination>(new PersonSource
        {
            FirstName = "Sam",
            LastName = "Fokam"
        });

        result.DisplayName.Should().Be("Sam Fokam");
    }

    [Fact]
    public void Map_Should_Not_Resolve_When_PreCondition_Is_False()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonSource, PersonDestination>()
            .ForMember(d => d.DisplayName, o =>
            {
                o.PreCondition(s => !string.IsNullOrWhiteSpace(s.FirstName));
                o.ResolveUsing(new FullNameResolver());
            });

        var mapper = CreateMapper(configuration);

        var destination = new PersonDestination
        {
            DisplayName = "KEEP"
        };

        mapper.Map(new PersonSource
        {
            FirstName = "",
            LastName = "Fokam"
        }, destination);

        destination.DisplayName.Should().Be("KEEP");
    }

    [Fact]
    public void Map_Should_Apply_NullSubstitute_After_Resolver()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonSource, PersonDestination>()
            .ForMember(d => d.DisplayName, o =>
            {
                o.ResolveUsing(new NullDisplayNameResolver());
                o.NullSubstitute("Unknown");
            });

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<PersonSource, PersonDestination>(new PersonSource
        {
            FirstName = "Sam",
            LastName = "Fokam"
        });

        result.DisplayName.Should().Be("Unknown");
    }

    [Fact]
    public void Map_Should_Keep_Condition_Behavior_After_PreCondition()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonSource, PersonDestination>()
            .ForMember(d => d.DisplayName, o =>
            {
                o.PreCondition(s => !string.IsNullOrWhiteSpace(s.FirstName));
                o.ResolveUsing(new FullNameResolver());
                o.Condition((src, dest) => src.LastName != "Blocked");
            });

        var mapper = CreateMapper(configuration);

        var destination = new PersonDestination
        {
            DisplayName = "KEEP"
        };

        mapper.Map(new PersonSource
        {
            FirstName = "Sam",
            LastName = "Blocked"
        }, destination);

        destination.DisplayName.Should().Be("KEEP");
    }

    private static ObjectMapper CreateMapper(MappingConfiguration configuration)
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
            new DomainRelay.Mapping.Abstractions.Configuration.MappingRuntimeOptions(),
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

    public sealed class PersonSource
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public sealed class PersonDestination
    {
        public string? DisplayName { get; set; }
    }

    private sealed class FullNameResolver : IValueResolver<PersonSource, PersonDestination, string?>
    {
        public string? Resolve(PersonSource source, PersonDestination destination)
        {
            return $"{source.FirstName} {source.LastName}".Trim();
        }
    }

    private sealed class NullDisplayNameResolver : IValueResolver<PersonSource, PersonDestination, string?>
    {
        public string? Resolve(PersonSource source, PersonDestination destination)
        {
            return null;
        }
    }
}