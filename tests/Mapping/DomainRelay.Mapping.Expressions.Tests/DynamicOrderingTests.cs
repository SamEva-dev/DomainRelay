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

public sealed class DynamicOrderingTests
{
    [Fact]
    public void OrderByTranslated_Should_Order_By_Destination_Member_Name_Ascending()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var source = CreateSource();

        var result = source
            .OrderByTranslated<AuditEntity, AuditDto>(
                nameof(AuditDto.Timestamp),
                descending: false,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(2, 3, 1);
    }

    [Fact]
    public void OrderByTranslated_Should_Order_By_Destination_Member_Name_Descending()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var source = CreateSource();

        var result = source
            .OrderByTranslated<AuditEntity, AuditDto>(
                nameof(AuditDto.Timestamp),
                descending: true,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(1, 3, 2);
    }

    [Fact]
    public void ThenByTranslated_Should_Order_By_Secondary_Destination_Member_Name()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var source = new[]
        {
            new AuditEntity { Id = 1, Action = "Delete", StatusCode = 500 },
            new AuditEntity { Id = 2, Action = "Create", StatusCode = 400 },
            new AuditEntity { Id = 3, Action = "Update", StatusCode = 400 }
        }.AsQueryable();

        var result = source
            .OrderByTranslated<AuditEntity, AuditDto>(
                nameof(AuditDto.HttpStatus),
                descending: false,
                translator)
            .ThenByTranslated<AuditEntity, AuditDto>(
                nameof(AuditDto.Action),
                descending: false,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(2, 3, 1);
    }

    [Fact]
    public void OrderByTranslated_Should_Use_Mapped_Destination_Member()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var source = new[]
        {
            new AuditEntity { Id = 1, StatusCode = 500 },
            new AuditEntity { Id = 2, StatusCode = 200 },
            new AuditEntity { Id = 3, StatusCode = 400 }
        }.AsQueryable();

        var result = source
            .OrderByTranslated<AuditEntity, AuditDto>(
                nameof(AuditDto.HttpStatus),
                descending: true,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(1, 3, 2);
    }

    [Fact]
    public void OrderByTranslated_Should_Throw_When_Destination_Member_Does_Not_Exist()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var source = CreateSource();

        var act = () => source
            .OrderByTranslated<AuditEntity, AuditDto>(
                "DoesNotExist",
                descending: false,
                translator)
            .ToList();

        act.Should()
            .Throw<ExpressionTranslationException>()
            .Where(ex => ex.DestinationType == typeof(AuditDto));
    }

    [Fact]
    public void OrderByTranslated_Should_Be_Case_Insensitive()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var source = CreateSource();

        var result = source
            .OrderByTranslated<AuditEntity, AuditDto>(
                "timestamp",
                descending: false,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(2, 3, 1);
    }

    private static IQueryable<AuditEntity> CreateSource()
    {
        return new[]
        {
            new AuditEntity { Id = 1, Action = "Delete", Timestamp = new DateTime(2026, 1, 3) },
            new AuditEntity { Id = 2, Action = "Create", Timestamp = new DateTime(2026, 1, 1) },
            new AuditEntity { Id = 3, Action = "Update", Timestamp = new DateTime(2026, 1, 2) }
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