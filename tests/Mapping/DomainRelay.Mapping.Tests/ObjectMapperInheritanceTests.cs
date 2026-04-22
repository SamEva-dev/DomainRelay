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

public sealed class ObjectMapperInheritanceTests
{
    [Fact]
    public void Map_Should_Use_Derived_Map_When_Base_Map_Includes_Derived()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();

        configuration.CreateMap<Dog, DogDto>();

        var mapper = CreateMapper(configuration);

        Animal source = new Dog
        {
            Name = "Rex",
            Breed = "Berger"
        };

        var result = mapper.Map<Animal, AnimalDto>(source);

        result.Should().BeOfType<DogDto>();
        result.Name.Should().Be("Rex");
        ((DogDto)result).Breed.Should().Be("Berger");
    }

    [Fact]
    public void Map_Should_Fall_Back_To_Base_Source_Map_When_Runtime_Source_Is_More_Derived()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<Animal, AnimalDto>();
        configuration.CreateMap<Dog, DogDto>();

        var mapper = CreateMapper(configuration);

        Animal source = new Bulldog
        {
            Name = "Max",
            Breed = "Bulldog anglais"
        };

        var result = mapper.Map(source, typeof(Animal), typeof(AnimalDto));

        result.Should().NotBeNull();
        result.Should().BeOfType<AnimalDto>();
        ((AnimalDto)result!).Name.Should().Be("Max");
    }

    [Fact]
    public void Map_Should_Use_Exact_Derived_Map_When_Requested_Destination_Is_Derived()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();

        configuration.CreateMap<Dog, DogDto>();

        var mapper = CreateMapper(configuration);

        Dog source = new Dog
        {
            Name = "Rex",
            Breed = "Berger"
        };

        var result = mapper.Map<Dog, DogDto>(source);

        result.Name.Should().Be("Rex");
        result.Breed.Should().Be("Berger");
    }

    [Fact]
    public void Map_Should_Keep_Base_Map_When_No_Included_Derived_Map_Is_Configured()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<Animal, AnimalDto>();
        configuration.CreateMap<Dog, DogDto>();

        var mapper = CreateMapper(configuration);

        Animal source = new Dog
        {
            Name = "Rex",
            Breed = "Berger"
        };

        var result = mapper.Map<Animal, AnimalDto>(source);

        result.Should().NotBeNull();
        result.Should().BeOfType<AnimalDto>();
        result.Name.Should().Be("Rex");
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

    public class Animal
    {
        public string? Name { get; set; }
    }

    public class Dog : Animal
    {
        public string? Breed { get; set; }
    }

    public sealed class Bulldog : Dog
    {
    }

    public class AnimalDto
    {
        public string? Name { get; set; }
    }

    public sealed class DogDto : AnimalDto
    {
        public string? Breed { get; set; }
    }
}