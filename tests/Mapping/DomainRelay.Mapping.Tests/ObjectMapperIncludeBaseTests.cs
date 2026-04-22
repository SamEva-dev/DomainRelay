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

public sealed class ObjectMapperIncludeBaseTests
{
    [Fact]
    public void IncludeBase_Should_Inherit_Base_Member_Configuration()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonBase, PersonBaseDto>()
            .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        configuration.CreateMap<Employee, EmployeeDto>()
            .IncludeBase<PersonBase, PersonBaseDto>();

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<Employee, EmployeeDto>(new Employee
        {
            FirstName = "Sam",
            LastName = "Fokam",
            Department = "IT"
        });

        result.DisplayName.Should().Be("Sam Fokam");
        result.Department.Should().Be("IT");
    }

    [Fact]
    public void IncludeBase_Should_Allow_Derived_Map_To_Override_Base_Member()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<PersonBase, PersonBaseDto>()
            .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        configuration.CreateMap<Employee, EmployeeDto>()
            .IncludeBase<PersonBase, PersonBaseDto>()
            .ForMember(d => d.DisplayName, o => o.MapFrom(s => "EMP-" + s.FirstName));

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<Employee, EmployeeDto>(new Employee
        {
            FirstName = "Sam",
            LastName = "Fokam",
            Department = "IT"
        });

        result.DisplayName.Should().Be("EMP-Sam");
        result.Department.Should().Be("IT");
    }

    [Fact]
    public void Polymorphic_Collections_Should_Map_Derived_Elements()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<Animal, AnimalDto>()
            .Include<Dog, DogDto>();

        configuration.CreateMap<Dog, DogDto>();

        var mapper = CreateMapper(configuration);

        var source = new List<Animal>
        {
            new Dog { Name = "Rex", Breed = "Berger" }
        };

        var result = mapper.Map<List<Animal>, List<AnimalDto>>(source);

        result.Should().HaveCount(1);
        result[0].Should().BeOfType<DogDto>();
        result[0].Name.Should().Be("Rex");
        ((DogDto)result[0]).Breed.Should().Be("Berger");
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

    public class PersonBase
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class Employee : PersonBase
    {
        public string? Department { get; set; }
    }

    public class PersonBaseDto
    {
        public string? DisplayName { get; set; }
    }

    public class EmployeeDto : PersonBaseDto
    {
        public string? Department { get; set; }
    }

    public class Animal
    {
        public string? Name { get; set; }
    }

    public class Dog : Animal
    {
        public string? Breed { get; set; }
    }

    public class AnimalDto
    {
        public string? Name { get; set; }
    }

    public class DogDto : AnimalDto
    {
        public string? Breed { get; set; }
    }
}