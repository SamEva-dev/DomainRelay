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

public sealed class QueryableProjectionExtensionsTests
{
    [Fact]
    public void ProjectTo_Should_Project_Source_To_Destination()
    {
        using var provider = CreateProvider();

        var projectionBuilder = provider.GetRequiredService<IProjectionBuilder>();

        var source = new[]
        {
            new AuditEntity
            {
                Id = 1,
                Action = "Create",
                StatusCode = 200,
                Timestamp = new DateTime(2026, 1, 1)
            }
        }.AsQueryable();

        var result = source
            .ProjectTo<AuditEntity, AuditDto>(projectionBuilder)
            .Single();

        result.Id.Should().Be(1);
        result.Action.Should().Be("Create");
        result.StatusCode.Should().Be(200);
        result.Timestamp.Should().Be(new DateTime(2026, 1, 1));
    }

    [Fact]
    public void ProjectTo_NonGeneric_Source_Should_Project_Source_To_Destination()
    {
        using var provider = CreateProvider();

        var projectionBuilder = provider.GetRequiredService<IProjectionBuilder>();

        IQueryable source = new[]
        {
            new AuditEntity
            {
                Id = 1,
                Action = "Create",
                StatusCode = 200,
                Timestamp = new DateTime(2026, 1, 1)
            }
        }.AsQueryable();

        var result = source
            .ProjectTo<AuditDto>(projectionBuilder)
            .Single();

        result.Id.Should().Be(1);
        result.Action.Should().Be("Create");
        result.StatusCode.Should().Be(200);
        result.Timestamp.Should().Be(new DateTime(2026, 1, 1));
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