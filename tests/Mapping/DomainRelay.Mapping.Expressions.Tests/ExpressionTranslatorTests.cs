using System.Linq.Expressions;
using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.Configuration;
using DomainRelay.Mapping.Expressions.Queryable;
using DomainRelay.Mapping.Expressions.Tests.Models;
using DomainRelay.Mapping.Expressions.Translation;
using FluentAssertions;

namespace DomainRelay.Mapping.Expressions.Tests;

public sealed class ExpressionTranslatorTests
{
    [Fact]
    public void Translate_Should_Map_Simple_Member_Predicate()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var translator = CreateTranslator(configuration);

        Expression<Func<UserDto, bool>> destinationExpression = x => x.FirstName == "Sam";

        var translated = translator.Translate<User, UserDto, bool>(destinationExpression);
        var compiled = translated.Compile();

        compiled(new User { FirstName = "Sam" }).Should().BeTrue();
        compiled(new User { FirstName = "Jean" }).Should().BeFalse();
    }

    [Fact]
    public void Translate_Should_Use_MapFrom_Expression()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDtoWithFullName>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        var translator = CreateTranslator(configuration);

        Expression<Func<UserDtoWithFullName, bool>> destinationExpression = x => x.FullName.Contains("Sam");

        var translated = translator.Translate<User, UserDtoWithFullName, bool>(destinationExpression);
        var compiled = translated.Compile();

        compiled(new User { FirstName = "Sam", LastName = "Fokam" }).Should().BeTrue();
        compiled(new User { FirstName = "Jean", LastName = "Dupont" }).Should().BeFalse();
    }

    [Fact]
    public void Translate_Should_Use_Flattened_Member()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserFlatDto>();

        var translator = CreateTranslator(configuration);

        Expression<Func<UserFlatDto, bool>> destinationExpression = x => x.AddressCity == "Paris";

        var translated = translator.Translate<User, UserFlatDto, bool>(destinationExpression);
        var compiled = translated.Compile();

        compiled(new User { Address = new Address { City = "Paris" } }).Should().BeTrue();
        compiled(new User { Address = new Address { City = "Berlin" } }).Should().BeFalse();
    }

    [Fact]
    public void Translate_Should_Map_String_Selector()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDtoWithFullName>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        var translator = CreateTranslator(configuration);

        Expression<Func<UserDtoWithFullName, string>> destinationExpression = x => x.FullName;

        var translated = translator.Translate<User, UserDtoWithFullName, string>(destinationExpression);
        var compiled = translated.Compile();

        var result = compiled(new User { FirstName = "Sam", LastName = "Fokam" });

        result.Should().Be("Sam Fokam");
    }

    [Fact]
    public void Translate_Should_Allow_Common_String_Methods()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var translator = CreateTranslator(configuration);

        Expression<Func<UserDto, bool>> destinationExpression = x => x.FirstName.ToLower().Trim() == "sam";

        var translated = translator.Translate<User, UserDto, bool>(destinationExpression);
        var compiled = translated.Compile();

        compiled(new User { FirstName = " Sam " }).Should().BeTrue();
        compiled(new User { FirstName = "Jean" }).Should().BeFalse();
    }

    [Fact]
    public void Translate_Should_Throw_For_Unsupported_Method()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var translator = CreateTranslator(configuration);

        Expression<Func<UserDto, bool>> destinationExpression = x => x.FirstName.Replace("a", "b") == "Sbm";

        var act = () => translator.Translate<User, UserDto, bool>(destinationExpression);

        act.Should().Throw<TranslationValidationException>();
    }

    [Fact]
    public void WhereTranslated_Should_Filter_Source_Queryable()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDto>();

        var translator = CreateTranslator(configuration);

        var users = new[]
        {
            new User { FirstName = "Sam" },
            new User { FirstName = "Jean" }
        }.AsQueryable();

        Expression<Func<UserDto, bool>> predicate = x => x.FirstName == "Sam";

        var result = users.WhereTranslated<User, UserDto>(predicate, translator).ToList();

        result.Should().HaveCount(1);
        result[0].FirstName.Should().Be("Sam");
    }

    [Fact]
    public void OrderByTranslated_Should_Order_Source_Queryable()
    {
        var configuration = new MappingConfiguration();
        configuration.CreateMap<User, UserDtoWithFullName>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FirstName + " " + s.LastName));

        var translator = CreateTranslator(configuration);

        var users = new[]
        {
            new User { FirstName = "Jean", LastName = "Dupont" },
            new User { FirstName = "Sam", LastName = "Fokam" }
        }.AsQueryable();

        Expression<Func<UserDtoWithFullName, string>> selector = x => x.FullName;

        var result = users.OrderByTranslated<User, UserDtoWithFullName, string>(selector, translator).ToList();

        result[0].FirstName.Should().Be("Jean");
        result[1].FirstName.Should().Be("Sam");
    }

    private static IExpressionTranslator CreateTranslator(MappingConfiguration configuration)
    {
        var planBuilder = new ExpressionTranslationPlanBuilder(configuration);
        var validator = new ExpressionTranslationValidator();
        return new DestinationToSourceExpressionTranslator(planBuilder, validator);
    }
}