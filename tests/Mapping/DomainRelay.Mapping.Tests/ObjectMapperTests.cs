using DomainRelay.Mapping.Abstractions.Exceptions;
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
using Xunit;

namespace DomainRelay.Mapping.Tests;

public sealed class ObjectMapperTests
{
    [Fact]
    public void Map_Should_Copy_Properties_By_Convention()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var mapper = CreateMapper(configuration);

        var source = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var result = mapper.Map<User, UserDto>(source);

        result.Id.Should().Be(source.Id);
        result.FirstName.Should().Be("Sam");
        result.LastName.Should().Be("Fokam");
    }

    [Fact]
    public void Map_Should_Copy_Properties_By_Convention_Ignoring_Case()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<CaseInsensitiveSource, CaseInsensitiveDestination>();

        var mapper = CreateMapper(configuration);

        var source = new CaseInsensitiveSource
        {
            firstname = "Sam",
            lastname = "Fokam"
        };

        var result = mapper.Map<CaseInsensitiveSource, CaseInsensitiveDestination>(source);

        result.FirstName.Should().Be("Sam");
        result.LastName.Should().Be("Fokam");
    }

    [Fact]
    public void Map_To_Existing_Instance_Should_Update_Target()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var mapper = CreateMapper(configuration);

        var source = new User
        {
            FirstName = "Jean",
            LastName = "Dupont"
        };

        var destination = new UserDto();

        mapper.Map(source, destination);

        destination.FirstName.Should().Be("Jean");
        destination.LastName.Should().Be("Dupont");
    }

    [Fact]
    public void CreateMap_Should_Throw_When_Map_Already_Exists()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var act = () => configuration.CreateMap<User, UserDto>();

        act.Should().Throw<MappingConfigurationException>();
    }

    [Fact]
    public void Map_Should_Apply_MapFrom_Configuration()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDtoWithFullName>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        var mapper = CreateMapper(configuration);

        var source = new User
        {
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var result = mapper.Map<User, UserDtoWithFullName>(source);

        result.FullName.Should().Be("Sam Fokam");
    }

    [Fact]
    public void Map_Should_Ignore_Configured_Destination_Member()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDtoWithSecret>()
            .Ignore(d => d.InternalSecret);

        var mapper = CreateMapper(configuration);

        var source = new User
        {
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var destination = new UserDtoWithSecret
        {
            InternalSecret = "UNCHANGED"
        };

        mapper.Map(source, destination);

        destination.FirstName.Should().Be("Sam");
        destination.LastName.Should().Be("Fokam");
        destination.InternalSecret.Should().Be("UNCHANGED");
    }

    [Fact]
    public void Map_Should_Ignore_Configured_Destination_Member_Using_ForMember_Options()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDtoWithSecret>()
            .ForMember(d => d.InternalSecret, o => o.Ignore());

        var mapper = CreateMapper(configuration);

        var source = new User
        {
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var destination = new UserDtoWithSecret
        {
            InternalSecret = "UNCHANGED"
        };

        mapper.Map(source, destination);

        destination.FirstName.Should().Be("Sam");
        destination.LastName.Should().Be("Fokam");
        destination.InternalSecret.Should().Be("UNCHANGED");
    }

    [Fact]
    public void Explicit_Member_Map_Should_Override_Convention()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>()
            .ForMember(d => d.FirstName, o => o.MapFrom(_ => "OVERRIDDEN"));

        var mapper = CreateMapper(configuration);

        var source = new User
        {
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var result = mapper.Map<User, UserDto>(source);

        result.FirstName.Should().Be("OVERRIDDEN");
        result.LastName.Should().Be("Fokam");
    }

    [Fact]
    public void Mapper_Should_Reuse_Cached_Plan()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var cache = new MappingPlanCache();
        var typeMapFactory = new TypeMapFactory(configuration);
        var planBuilder = new MappingPlanBuilder();
        var validator = new MappingValidator();
        var converterRegistry = CreateConverterRegistry();

        var mapper = new ObjectMapper(
            configuration,
            typeMapFactory,
            planBuilder,
            cache,
            new CompiledMappingPlanCache(),
            validator,
            converterRegistry,
            new CollectionMapper(),
            new DictionaryMapper(),
            new MappingRuntimeOptions(),
            new InMemoryMappingDiagnosticsCollector());

        var source = new User
        {
            FirstName = "Sam",
            LastName = "Fokam"
        };

        mapper.Map<User, UserDto>(source);
        mapper.Map<User, UserDto>(source);

        cache.TryGet(typeof(User), typeof(UserDto), out var cached).Should().BeTrue();
        cached.Should().NotBeNull();
    }

    [Fact]
    public void Map_Should_Apply_Condition_When_Configured()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDtoWithNickname>()
            .ForMember(d => d.Nickname, o =>
            {
                o.MapFrom(s => s.FirstName);
                o.Condition((src, dest) => !string.IsNullOrWhiteSpace(src.FirstName));
            });

        var mapper = CreateMapper(configuration);

        var source = new User
        {
            FirstName = ""
        };

        var destination = new UserDtoWithNickname
        {
            Nickname = "KEEP"
        };

        mapper.Map(source, destination);

        destination.Nickname.Should().Be("KEEP");
    }

    [Fact]
    public void Map_Should_Use_NullSubstitute_When_Source_Value_Is_Null()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>()
            .ForMember(d => d.FirstName, o =>
            {
                o.MapFrom(s => (string?)null!);
                o.NullSubstitute("Unknown");
            });

        var mapper = CreateMapper(configuration);

        var source = new User
        {
            FirstName = "Ignored"
        };

        var result = mapper.Map<User, UserDto>(source);

        result.FirstName.Should().Be("Unknown");
    }

    [Fact]
    public void Map_Should_Use_NullSubstitute_With_ValueConverter_When_Source_Value_Is_Null()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<UserAgeSource, UserAgeDestination>()
            .ForMember(d => d.Age, o =>
            {
                o.MapFrom(s => (string)null!);
                o.ConvertUsing(new NullableIntToStringValueConverter());
                o.NullSubstitute("Age:unknown");
            });

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<UserAgeSource, UserAgeDestination>(new UserAgeSource { Age = 30 });

        result.Age.Should().Be("Age:unknown");
    }

    [Fact]
    public void Map_Should_Use_Global_TypeConverter_When_Types_Are_Not_Assignable()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<UserWithAge, UserDtoWithAgeText>();

        var mapper = CreateMapper(configuration);

        var source = new UserWithAge
        {
            AgeText = 42
        };

        var result = mapper.Map<UserWithAge, UserDtoWithAgeText>(source);

        result.AgeText.Should().Be("42");
    }

    [Fact]
    public void Map_Should_Use_Member_ValueConverter_When_Configured()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<UserAgeSource, UserAgeDestination>()
            .ForMember(d => d.Age, o =>
            {
                o.MapFrom(s => s.Age.ToString());
                o.ConvertUsing(new IntToStringValueConverter());
            });

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<UserAgeSource, UserAgeDestination>(new UserAgeSource { Age = 30 });

        result.Age.Should().Be("Age:30");
    }

    [Fact]
    public void Map_Should_Map_Nested_Object()
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

        var result = mapper.Map<UserWithAddress, UserWithAddressDto>(source);

        result.Address.Should().NotBeNull();
        result.Address!.City.Should().Be("Paris");
        result.Address.Street.Should().Be("10 rue A");
    }

    [Fact]
    public void Map_Should_Map_List_Of_Complex_Objects()
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

        var result = mapper.Map<UserWithOrders, UserWithOrdersDto>(source);

        result.Orders.Should().HaveCount(2);
        result.Orders[0].Number.Should().Be("A1");
        result.Orders[1].Number.Should().Be("A2");
    }

    [Fact]
    public void Map_Should_Map_Array()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<Order, OrderDto>();

        var mapper = CreateMapper(configuration);

        var source = new[]
        {
            new Order { Number = "X1" },
            new Order { Number = "X2" }
        };

        var result = (OrderDto[])mapper.Map(source, source.GetType(), typeof(OrderDto[]))!;

        result.Should().HaveCount(2);
        result[0].Number.Should().Be("X1");
        result[1].Number.Should().Be("X2");
    }

    [Fact]
    public void Map_Should_Map_Object_To_Dictionary()
    {
        var configuration = new MappingConfiguration();
        var mapper = CreateMapper(configuration);

        var source = new User
        {
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var result = mapper.Map<Dictionary<string, object?>>(source);

        result["FirstName"].Should().Be("Sam");
        result["LastName"].Should().Be("Fokam");
    }

    [Fact]
    public void Map_Should_Map_Dictionary_To_Object()
    {
        var configuration = new MappingConfiguration();
        var mapper = CreateMapper(configuration);

        var source = new Dictionary<string, object?>
        {
            ["FirstName"] = "Sam",
            ["LastName"] = "Fokam"
        };

        var result = mapper.Map<User>(source);

        result.FirstName.Should().Be("Sam");
        result.LastName.Should().Be("Fokam");
    }

    [Fact]
    public void Map_Should_Execute_BeforeMap_And_AfterMap()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<User, UserDtoWithTrace>()
            .BeforeMap((src, dest) => dest.Trace = "before")
            .AfterMap((src, dest) => dest.Trace += "|after");

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<User, UserDtoWithTrace>(new User());

        result.Trace.Should().Be("before|after");
    }

    [Fact]
    public void ReverseMap_Should_Create_Reverse_Mapping_For_Simple_Members()
    {
        var configuration = new MappingConfiguration();

        configuration.CreateMap<User, UserDto>()
            .ReverseMap();

        var mapper = CreateMapper(configuration);

        var dto = new UserDto
        {
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var result = mapper.Map<UserDto, User>(dto);

        result.FirstName.Should().Be("Sam");
        result.LastName.Should().Be("Fokam");
    }

    [Fact]
    public void Map_Should_Convert_String_To_Enum()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<UserStatusSource, UserStatusDestination>();

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<UserStatusSource, UserStatusDestination>(
            new UserStatusSource { Status = "Active" });

        result.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Map_Should_Convert_Enum_To_String()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<UserStatusDestination, UserStatusTextDestination>();

        var mapper = CreateMapper(configuration);

        var result = mapper.Map<UserStatusDestination, UserStatusTextDestination>(
            new UserStatusDestination { Status = UserStatus.Active });

        result.Status.Should().Be("Active");
    }

    [Fact]
    public void Map_Should_Handle_Basic_Cycle_Without_Stack_Overflow()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<Node, NodeDto>();

        var mapper = CreateMapper(configuration);

        var root = new Node { Name = "root" };
        root.Child = root;

        var result = mapper.Map<Node, NodeDto>(root);

        result.Name.Should().Be("root");
        result.Child.Should().NotBeNull();
        result.Child.Should().BeSameAs(result);
    }

    [Fact]
    public void Map_Should_Flatten_Nested_Source_Members()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserFlatDto>();

        var mapper = CreateMapper(configuration);

        var source = new User
        {
            Address = new Address
            {
                City = "Paris"
            }
        };

        var result = mapper.Map<User, UserFlatDto>(source);

        result.AddressCity.Should().Be("Paris");
    }

    [Fact]
    public void Map_Should_Unflatten_Source_Member_Into_Nested_Destination()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<UserFlatDto, User>();

        var mapper = CreateMapper(configuration);

        var source = new UserFlatDto
        {
            AddressCity = "Berlin"
        };

        var result = mapper.Map<UserFlatDto, User>(source);

        result.Address.Should().NotBeNull();
        result.Address!.City.Should().Be("Berlin");
    }

    [Fact]
    public void Map_Should_Use_Constructor_For_Record_Destination()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserSummaryDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        var mapper = CreateMapper(configuration);

        var source = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var result = mapper.Map<User, UserSummaryDto>(source);

        result.Id.Should().Be(source.Id);
        result.FullName.Should().Be("Sam Fokam");
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

    private sealed class CaseInsensitiveSource
    {
        public string firstname { get; set; } = string.Empty;
        public string lastname { get; set; } = string.Empty;
    }

    private sealed class CaseInsensitiveDestination
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    private sealed class NullableIntToStringValueConverter : DomainRelay.Mapping.Abstractions.Converters.IValueConverter<string?>
    {
        public object? Convert(string? sourceMember)
        {
            return sourceMember is null ? null : $"Age:{sourceMember}";
        }
    }
}