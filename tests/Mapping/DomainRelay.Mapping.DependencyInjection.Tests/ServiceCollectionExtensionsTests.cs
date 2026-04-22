using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Abstractions.Generation;
using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using DomainRelay.Mapping.DependencyInjection.Tests.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Mapping.DependencyInjection.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDomainRelayMapping_Should_Register_IObjectMapper()
    {
        var services = new ServiceCollection();

        services.AddDomainRelayMapping(builder =>
        {
            builder.AddProfile<UserProfile>();
        });

        var provider = services.BuildServiceProvider();

        var mapper = provider.GetService<IObjectMapper>();

        mapper.Should().NotBeNull();
    }

    [Fact]
    public void AddDomainRelayMapping_Should_Register_IMappingConfiguration()
    {
        var services = new ServiceCollection();

        services.AddDomainRelayMapping(builder =>
        {
            builder.AddProfile<UserProfile>();
        });

        var provider = services.BuildServiceProvider();

        var configuration = provider.GetService<IMappingConfiguration>();

        configuration.Should().NotBeNull();
    }

    [Fact]
    public void AddDomainRelayMapping_Should_Map_Configured_Profile()
    {
        var services = new ServiceCollection();

        services.AddDomainRelayMapping(builder =>
        {
            builder.AddProfile<UserProfile>();
        });

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IObjectMapper>();

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
    public void AddDomainRelayMapping_Should_Add_Profiles_From_Assembly_Containing()
    {
        var services = new ServiceCollection();

        services.AddDomainRelayMapping(builder =>
        {
            builder.AddProfilesFromAssemblyContaining<UserProfile>();
        });

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IObjectMapper>();

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
    public void AddDomainRelayMapping_Should_Validate_Configuration_On_Build_When_Requested()
    {
        var services = new ServiceCollection();

        var act = () => services.AddDomainRelayMapping(builder =>
        {
            builder.AddProfile<InvalidProfile>();
            builder.ValidateConfigurationOnBuild();
        });

        act.Should().Throw<MappingValidationException>();
    }

    [Fact]
    public void AddDomainRelayMapping_Should_Register_And_Use_Generated_Mapping_Registry()
    {
        var services = new ServiceCollection();

        services.AddDomainRelayMapping(builder =>
        {
            builder.AddGeneratedMappingRegistry<FakeGeneratedMappingRegistry>();
        });

        var provider = services.BuildServiceProvider();

        var registry = provider.GetRequiredService<IGeneratedMappingRegistry>();
        var mapper = provider.GetRequiredService<IObjectMapper>();

        registry.Should().NotBeNull();

        var source = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var result = mapper.Map<GeneratedUserDto>(source);

        result.Id.Should().Be(source.Id);
        result.FullName.Should().Be("Sam Fokam");
    }

    [Fact]
    public void AddDomainRelayMapping_Should_Compose_Multiple_Generated_Mapping_Registries()
    {
        var services = new ServiceCollection();

        services.AddDomainRelayMapping(builder =>
        {
            builder.AddGeneratedMappingRegistry<FakeGeneratedMappingRegistry>();
            builder.AddGeneratedMappingRegistry<SecondFakeGeneratedMappingRegistry>();
        });

        var provider = services.BuildServiceProvider();
        var mapper = provider.GetRequiredService<IObjectMapper>();

        var source = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var generated = mapper.Map<GeneratedUserDto>(source);
        var secondary = mapper.Map<SecondaryGeneratedUserDto>(source);

        generated.FullName.Should().Be("Sam Fokam");
        secondary.DisplayName.Should().Be("FOKAM, Sam");
    }

    private sealed class InvalidProfile : MappingProfile
    {
        public override void Configure(IMappingConfiguration configuration)
        {
            configuration.CreateMap<BrokenSource, BrokenDestination>();
        }
    }

    private sealed class BrokenSource
    {
        public string? Value { get; set; }
    }

    private sealed class BrokenDestination
    {
        public string Value { get; }

        public BrokenDestination(string value)
        {
            Value = value;
        }
    }

    public sealed class GeneratedUserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public sealed class SecondaryGeneratedUserDto
    {
        public string DisplayName { get; set; } = string.Empty;
    }

    public sealed class FakeGeneratedMappingRegistry : IGeneratedMappingRegistry
    {
        public bool TryGetGeneratedMapper(Type sourceType, Type destinationType, out Func<object, object>? mapper)
        {
            if (sourceType == typeof(User) && destinationType == typeof(GeneratedUserDto))
            {
                mapper = source =>
                {
                    var user = (User)source;
                    return new GeneratedUserDto
                    {
                        Id = user.Id,
                        FullName = $"{user.FirstName} {user.LastName}"
                    };
                };

                return true;
            }

            mapper = null;
            return false;
        }
    }

    public sealed class SecondFakeGeneratedMappingRegistry : IGeneratedMappingRegistry
    {
        public bool TryGetGeneratedMapper(Type sourceType, Type destinationType, out Func<object, object>? mapper)
        {
            if (sourceType == typeof(User) && destinationType == typeof(SecondaryGeneratedUserDto))
            {
                mapper = source =>
                {
                    var user = (User)source;
                    return new SecondaryGeneratedUserDto
                    {
                        DisplayName = $"{user.LastName.ToUpperInvariant()}, {user.FirstName}"
                    };
                };

                return true;
            }

            mapper = null;
            return false;
        }
    }
}