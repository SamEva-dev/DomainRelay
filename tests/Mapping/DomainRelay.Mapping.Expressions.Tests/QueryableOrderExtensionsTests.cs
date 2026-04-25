using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using DomainRelay.Mapping.Expressions.Extensions;
using DomainRelay.Mapping.Expressions.Queryable;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DomainRelay.Mapping.Expressions.Tests;

public sealed class QueryableOrderExtensionsTests
{
    [Fact]
    public void OrderByTranslated_Should_Order_Source_By_Destination_Property()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var source = new[]
        {
            new AuditEntity { Id = 1, Action = "Delete", Timestamp = new DateTime(2026, 1, 3) },
            new AuditEntity { Id = 2, Action = "Create", Timestamp = new DateTime(2026, 1, 1) },
            new AuditEntity { Id = 3, Action = "Update", Timestamp = new DateTime(2026, 1, 2) }
        }.AsQueryable();

        var result = source
            .OrderByTranslated<AuditEntity, AuditDto, DateTime>(
                dto => dto.Timestamp,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(2, 3, 1);
    }

    [Fact]
    public void OrderByDescendingTranslated_Should_Order_Source_Descending_By_Destination_Property()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var source = new[]
        {
            new AuditEntity { Id = 1, Action = "Delete", Timestamp = new DateTime(2026, 1, 3) },
            new AuditEntity { Id = 2, Action = "Create", Timestamp = new DateTime(2026, 1, 1) },
            new AuditEntity { Id = 3, Action = "Update", Timestamp = new DateTime(2026, 1, 2) }
        }.AsQueryable();

        var result = source
            .OrderByDescendingTranslated<AuditEntity, AuditDto, DateTime>(
                dto => dto.Timestamp,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(1, 3, 2);
    }

    [Fact]
    public void ThenByTranslated_Should_Apply_Secondary_Order()
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
            .OrderByTranslated<AuditEntity, AuditDto, int>(
                dto => dto.StatusCode,
                translator)
            .ThenByTranslated<AuditEntity, AuditDto, string>(
                dto => dto.Action,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(2, 3, 1);
    }

    [Fact]
    public void ThenByDescendingTranslated_Should_Apply_Secondary_Descending_Order()
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
            .OrderByTranslated<AuditEntity, AuditDto, int>(
                dto => dto.StatusCode,
                translator)
            .ThenByDescendingTranslated<AuditEntity, AuditDto, string>(
                dto => dto.Action,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(3, 2, 1);
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
            configuration.CreateMap<AuditEntity, AuditDto>();
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
        public DateTime Timestamp { get; set; }
    }
}