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

public sealed class QueryableTranslationExtensionsTests
{
    [Fact]
    public void WhereTranslated_Should_Filter_Source_Using_Destination_Predicate()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var source = new[]
        {
            new AuditEntity { Id = 1, Action = "Create", StatusCode = 200 },
            new AuditEntity { Id = 2, Action = "Delete", StatusCode = 500 },
            new AuditEntity { Id = 3, Action = "Update", StatusCode = 400 }
        }.AsQueryable();

        var result = source
            .WhereTranslated<AuditEntity, AuditDto>(
                dto => dto.StatusCode >= 400,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(2, 3);
    }

    [Fact]
    public void WhereTranslated_Should_Filter_Source_Using_Mapped_Destination_Member()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var source = new[]
        {
            new AuditEntity { Id = 1, Action = "Create", StatusCode = 200 },
            new AuditEntity { Id = 2, Action = "Delete", StatusCode = 500 },
            new AuditEntity { Id = 3, Action = "Update", StatusCode = 400 }
        }.AsQueryable();

        var result = source
            .WhereTranslated<AuditEntity, AuditDto>(
                dto => dto.HttpStatus >= 400,
                translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(2, 3);
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
    }

    private sealed class AuditDto
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public int HttpStatus { get; set; }
    }
}