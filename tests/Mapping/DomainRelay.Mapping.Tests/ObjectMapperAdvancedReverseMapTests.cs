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

public sealed class ObjectMapperAdvancedReverseMapTests
{
    [Fact]
    public void ReverseMap_Should_Reverse_Direct_Renamed_Member()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonEntity, PersonDto>()
            .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.Name))
            .ReverseMap();

        var mapper = CreateMapper(configuration);

        var dto = new PersonDto
        {
            DisplayName = "Sam"
        };

        var entity = mapper.Map<PersonDto, PersonEntity>(dto);

        entity.Name.Should().Be("Sam");
    }

    [Fact]
    public void ReverseMap_Should_Propagate_Ignore_For_Same_Named_Member()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonWithAuditEntity, PersonWithAuditDto>()
            .Ignore(d => d.AuditCode)
            .ReverseMap();

        var mapper = CreateMapper(configuration);

        var dto = new PersonWithAuditDto
        {
            Name = "Sam",
            AuditCode = "SHOULD_NOT_BE_MAPPED"
        };

        var entity = new PersonWithAuditEntity
        {
            AuditCode = "KEEP"
        };

        mapper.Map(dto, entity);

        entity.Name.Should().Be("Sam");
        entity.AuditCode.Should().Be("KEEP");
    }

    [Fact]
    public void ReverseMap_Should_Not_Break_When_Forward_MapFrom_Is_Complex()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonEntity, PersonDto>()
            .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.Name + " Smith"))
            .ReverseMap();

        var mapper = CreateMapper(configuration);

        var dto = new PersonDto
        {
            DisplayName = "Sam Smith"
        };

        var entity = mapper.Map<PersonDto, PersonEntity>(dto);

        entity.Name.Should().BeNull();
    }

    [Fact]
    public void ReverseMap_Should_Still_Handle_Convention_Members()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonEntity, PersonDtoWithId>()
            .ReverseMap();

        var mapper = CreateMapper(configuration);

        var dto = new PersonDtoWithId
        {
            Id = Guid.NewGuid(),
            Name = "Sam"
        };

        var entity = mapper.Map<PersonDtoWithId, PersonEntityWithId>(dto);

        entity.Id.Should().Be(dto.Id);
        entity.Name.Should().Be(dto.Name);
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

    public sealed class PersonEntity
    {
        public string? Name { get; set; }
    }

    public sealed class PersonDto
    {
        public string? DisplayName { get; set; }
    }

    public sealed class PersonWithAuditDto
    {
        public string? Name { get; set; }
        public string? AuditCode { get; set; }
    }

    public sealed class PersonWithAuditEntity
    {
        public string? Name { get; set; }
        public string? AuditCode { get; set; }
    }

    public sealed class PersonEntityWithId
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    public sealed class PersonDtoWithId
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}