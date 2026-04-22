using DomainRelay.Mapping.Abstractions.Configuration;
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

public sealed class ObjectMapperCollectionsAndDictionariesTests
{
    [Fact]
    public void Map_Should_Update_Existing_List_Destination()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<Order, OrderDto>();

        var mapper = CreateMapper(configuration);

        var source = new List<Order>
        {
            new() { Number = "N1" },
            new() { Number = "N2" }
        };

        var destination = new List<OrderDto>
        {
            new() { Number = "OLD" }
        };

        mapper.Map(source, destination, typeof(List<Order>), typeof(List<OrderDto>));

        destination.Should().HaveCount(2);
        destination[0].Number.Should().Be("N1");
        destination[1].Number.Should().Be("N2");
    }

    [Fact]
    public void Map_Should_Update_Existing_Nested_Object_When_Destination_Instance_Already_Exists()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<Address, AddressDto>();
        configuration.CreateMap<UserWithAddress, UserWithAddressDto>();

        var mapper = CreateMapper(configuration);

        var source = new UserWithAddress
        {
            FirstName = "Sam",
            Address = new Address
            {
                City = "Paris",
                Street = "10 rue A"
            }
        };

        var destination = new UserWithAddressDto
        {
            FirstName = "Old",
            Address = new AddressDto
            {
                City = "OldCity",
                Street = "OldStreet"
            }
        };

        mapper.Map(source, destination);

        destination.FirstName.Should().Be("Sam");
        destination.Address.Should().NotBeNull();
        destination.Address!.City.Should().Be("Paris");
        destination.Address.Street.Should().Be("10 rue A");
    }

    [Fact]
    public void Map_Should_Update_Existing_Nested_List_Property_When_Destination_Instance_Already_Exists()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<Order, OrderDto>();
        configuration.CreateMap<UserWithOrders, UserWithOrdersDto>();

        var mapper = CreateMapper(configuration);

        var source = new UserWithOrders
        {
            Orders = new List<Order>
            {
                new() { Number = "A1" },
                new() { Number = "A2" }
            }
        };

        var destination = new UserWithOrdersDto
        {
            Orders = new List<OrderDto>
            {
                new() { Number = "OLD" }
            }
        };

        mapper.Map(source, destination);

        destination.Orders.Should().HaveCount(2);
        destination.Orders[0].Number.Should().Be("A1");
        destination.Orders[1].Number.Should().Be("A2");
    }

    [Fact]
    public void Map_Should_Update_Existing_Object_From_Dictionary()
    {
        var configuration = new MappingConfiguration();
        var mapper = CreateMapper(configuration);

        var source = new Dictionary<string, object?>
        {
            ["FirstName"] = "Sam",
            ["LastName"] = "Fokam"
        };

        var destination = new User
        {
            FirstName = "Old",
            LastName = "Name"
        };

        mapper.Map(source, destination, typeof(Dictionary<string, object?>), typeof(User));

        destination.FirstName.Should().Be("Sam");
        destination.LastName.Should().Be("Fokam");
    }

    [Fact]
    public void Map_Should_Update_Existing_Dictionary_From_Object()
    {
        var configuration = new MappingConfiguration();
        var mapper = CreateMapper(configuration);

        var source = new User
        {
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var destination = new Dictionary<string, object?>
        {
            ["Legacy"] = "to-remove"
        };

        mapper.Map(source, destination, typeof(User), typeof(Dictionary<string, object?>));

        destination.Should().ContainKey("FirstName");
        destination.Should().ContainKey("LastName");
        destination["FirstName"].Should().Be("Sam");
        destination["LastName"].Should().Be("Fokam");
        destination.ContainsKey("Legacy").Should().BeFalse();
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
            new MappingRuntimeOptions(),
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
}