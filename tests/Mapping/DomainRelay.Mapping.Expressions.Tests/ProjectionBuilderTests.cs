using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.Configuration;
using DomainRelay.Mapping.Expressions.Projection;
using DomainRelay.Mapping.Expressions.Queryable;
using DomainRelay.Mapping.Expressions.Translation;
using DomainRelay.Mapping.Expressions.Tests.Models;
using FluentAssertions;
using Xunit;

namespace DomainRelay.Mapping.Expressions.Tests;

public sealed class ProjectionBuilderTests
{
    [Fact]
    public void BuildProjection_Should_Create_Simple_MemberInit()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var builder = CreateProjectionBuilder(configuration);

        var projection = builder.BuildProjection<User, UserDto>();
        var compiled = projection.Compile();

        var result = compiled(new User
        {
            FirstName = "Sam",
            LastName = "Fokam"
        });

        result.FirstName.Should().Be("Sam");
        result.LastName.Should().Be("Fokam");
    }

    [Fact]
    public void BuildProjection_Should_Use_MapFrom_Expression()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDtoWithFullName>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        var builder = CreateProjectionBuilder(configuration);

        var projection = builder.BuildProjection<User, UserDtoWithFullName>().Compile();

        var result = projection(new User
        {
            FirstName = "Sam",
            LastName = "Fokam"
        });

        result.FullName.Should().Be("Sam Fokam");
    }

    [Fact]
    public void BuildProjection_Should_Flatten_Nested_Properties()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserFlatDto>();

        var builder = CreateProjectionBuilder(configuration);

        var projection = builder.BuildProjection<User, UserFlatDto>().Compile();

        var result = projection(new User
        {
            Address = new Address
            {
                City = "Paris"
            }
        });

        result.AddressCity.Should().Be("Paris");
    }

    [Fact]
    public void BuildProjection_Should_Project_Nested_Object_When_Nested_Map_Exists()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<Address, AddressDto>();
        configuration.CreateMap<User, UserWithAddressDto>();

        var builder = CreateProjectionBuilder(configuration);

        var projection = builder.BuildProjection<User, UserWithAddressDto>().Compile();

        var result = projection(new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Sam",
            Address = new Address
            {
                City = "Paris",
                Street = "10 rue A"
            }
        });

        result.FirstName.Should().Be("Sam");
        result.Address.Should().NotBeNull();
        result.Address.City.Should().Be("Paris");
        result.Address.Street.Should().Be("10 rue A");
    }

    [Fact]
    public void BuildProjection_Should_Use_Record_Constructor()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserSummaryDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        var builder = CreateProjectionBuilder(configuration);

        var projection = builder.BuildProjection<User, UserSummaryDto>().Compile();

        var source = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Sam",
            LastName = "Fokam"
        };

        var result = projection(source);

        result.Id.Should().Be(source.Id);
        result.FullName.Should().Be("Sam Fokam");
    }

    [Fact]
    public void ProjectTo_Should_Project_Queryable()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var builder = CreateProjectionBuilder(configuration);

        var source = new[]
        {
            new User { FirstName = "Sam", LastName = "Fokam" },
            new User { FirstName = "Jean", LastName = "Dupont" }
        }.AsQueryable();

        var result = source.ProjectTo<User, UserDto>(builder).ToList();

        result.Should().HaveCount(2);
        result[0].FirstName.Should().Be("Sam");
        result[1].FirstName.Should().Be("Jean");
    }

    private static IProjectionBuilder CreateProjectionBuilder(MappingConfiguration configuration)
    {
        var planBuilder = new ProjectionPlanBuilder(configuration);
        var validator = new ProjectionValidator();
        return new ProjectionBuilder(planBuilder, validator);
    }
}