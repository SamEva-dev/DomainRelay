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

public sealed class ObjectMapperCtorParamTests
{
    [Fact]
    public void Map_Should_Use_ForCtorParam_For_Record_Destination()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonSource, PersonRecordDto>()
            .ForCtorParam<string?>("fullName", o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<PersonSource, PersonRecordDto>(new PersonSource
        {
            Id = Guid.NewGuid(),
            FirstName = "Sam",
            LastName = "Fokam"
        });

        result.FullName.Should().Be("Sam Fokam");
    }

    [Fact]
    public void Map_Should_Use_ForCtorParam_NullSubstitute()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonSource, PersonRecordDto>()
            .ForCtorParam<string?>("fullName", o =>
            {
                o.MapFrom(s => (string?)null!);
                o.NullSubstitute("Unknown");
            });

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<PersonSource, PersonRecordDto>(new PersonSource
        {
            Id = Guid.NewGuid(),
            FirstName = "Sam",
            LastName = "Fokam"
        });

        result.FullName.Should().Be("Unknown");
    }

    [Fact]
    public void Map_Should_Prefer_ForCtorParam_Over_Convention_For_Constructor_Parameter()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonSource, PersonRecordDto>()
            .ForCtorParam<string?>("fullName", o => o.MapFrom(s => "OVERRIDE"));

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<PersonSource, PersonRecordDto>(new PersonSource
        {
            Id = Guid.NewGuid(),
            FirstName = "Sam",
            LastName = "Fokam"
        });

        result.FullName.Should().Be("OVERRIDE");
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
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public sealed record PersonRecordDto(Guid Id, string? FullName);
}