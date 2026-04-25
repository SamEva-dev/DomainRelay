using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using DomainRelay.Mapping.Expressions.Extensions;
using DomainRelay.Mapping.Expressions.Queryable;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DomainRelay.Mapping.Expressions.Tests;

public sealed class DynamicFilterTests
{
    [Fact]
    public void WhereTranslatedEquals_Should_Filter_By_Destination_Member_Name()
    {
        using var provider = CreateProvider();
        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var result = CreateSource()
            .WhereTranslatedEquals<AuditEntity, AuditDto>(
                nameof(AuditDto.Action),
                "Delete",
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(2);
    }

    [Fact]
    public void WhereTranslatedEquals_Should_Be_Case_Insensitive_For_Member_Name()
    {
        using var provider = CreateProvider();
        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var result = CreateSource()
            .WhereTranslatedEquals<AuditEntity, AuditDto>(
                "action",
                "Delete",
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(2);
    }

    [Fact]
    public void WhereTranslatedGreaterThanOrEqual_Should_Filter_By_Mapped_Destination_Member()
    {
        using var provider = CreateProvider();
        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var result = CreateSource()
            .WhereTranslatedGreaterThanOrEqual<AuditEntity, AuditDto>(
                nameof(AuditDto.HttpStatus),
                400,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(2, 3);
    }

    [Fact]
    public void WhereTranslatedLessThan_Should_Filter_By_Date()
    {
        using var provider = CreateProvider();
        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var result = CreateSource()
            .WhereTranslatedLessThan<AuditEntity, AuditDto>(
                nameof(AuditDto.Timestamp),
                new DateTime(2026, 1, 3),
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(1, 3);
    }

    [Fact]
    public void WhereTranslatedStringContains_Should_Filter_String_Member()
    {
        using var provider = CreateProvider();
        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var result = CreateSource()
            .WhereTranslatedStringContains<AuditEntity, AuditDto>(
                nameof(AuditDto.Action),
                "eat",
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(1);
    }

    [Fact]
    public void WhereTranslatedEquals_Should_Throw_When_Member_Does_Not_Exist()
    {
        using var provider = CreateProvider();
        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var act = () => CreateSource()
            .WhereTranslatedEquals<AuditEntity, AuditDto>(
                "DoesNotExist",
                "x",
                translator)
            .ToList();

        act.Should()
            .Throw<ExpressionTranslationException>()
            .Where(ex => ex.DestinationType == typeof(AuditDto));
    }

    private static IQueryable<AuditEntity> CreateSource()
    {
        return new[]
        {
            new AuditEntity
            {
                Id = 1,
                Action = "Create",
                StatusCode = 200,
                Timestamp = new DateTime(2026, 1, 1)
            },
            new AuditEntity
            {
                Id = 2,
                Action = "Delete",
                StatusCode = 500,
                Timestamp = new DateTime(2026, 1, 3)
            },
            new AuditEntity
            {
                Id = 3,
                Action = "Update",
                StatusCode = 400,
                Timestamp = new DateTime(2026, 1, 2)
            }
        }.AsQueryable();
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();

        services.AddDomainRelayMapping(mapping =>
        {
            mapping
                .AddProfile<AuditMappingProfile>()
                .ValidateConfigurationOnBuild();
        });

        services.AddDomainRelayMappingExpressions();

        return services.BuildServiceProvider();
    }

    private sealed class AuditMappingProfile : MappingProfile
    {
        public override void Configure(IMappingConfiguration configuration)
        {
            configuration.CreateMap<AuditEntity, AuditDto>()
                .ForMember(d => d.HttpStatus, opt => opt.MapFrom(s => s.StatusCode));
        }
    }

    private sealed class AuditEntity
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
    }

    private sealed class AuditDto
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public int HttpStatus { get; set; }
        public DateTime Timestamp { get; set; }
    }
}