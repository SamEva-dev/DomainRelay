using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using DomainRelay.Mapping.Expressions.Dynamic;
using DomainRelay.Mapping.Expressions.Extensions;
using DomainRelay.Mapping.Expressions.Queryable;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DomainRelay.Mapping.Expressions.Tests;

public sealed class DynamicQueryOptionsTests
{
    [Fact]
    public void ApplyDynamicQuery_Should_Apply_Filters_And_Sorts()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var options = new DynamicQueryOptions();

        options.Filters.Add(new DynamicFilter(
            nameof(AuditDto.HttpStatus),
            DynamicFilterOperator.GreaterThanOrEqual,
            400));

        options.Sorts.Add(new DynamicSort(
            nameof(AuditDto.Timestamp),
            DynamicSortDirection.Desc));

        var result = CreateSource()
            .ApplyDynamicQuery<AuditEntity, AuditDto>(
                options,
                translator,
                fallbackSort: new DynamicSort(nameof(AuditDto.Id)))
            .ToList();

        result.Select(x => x.Id).Should().Equal(2, 3);
    }

    [Fact]
    public void ApplyDynamicSorts_Should_Use_Fallback_When_No_Sort_Is_Provided()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var result = CreateSource()
            .ApplyDynamicSorts<AuditEntity, AuditDto>(
                Array.Empty<DynamicSort>(),
                translator,
                fallbackSort: new DynamicSort(nameof(AuditDto.Timestamp), DynamicSortDirection.Asc))
            .ToList();

        result.Select(x => x.Id).Should().Equal(1, 3, 2);
    }

    [Fact]
    public void ApplyDynamicFilters_Should_Apply_Multiple_Filters()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var filters = new[]
        {
            new DynamicFilter(
                nameof(AuditDto.HttpStatus),
                DynamicFilterOperator.GreaterThanOrEqual,
                400),
            new DynamicFilter(
                nameof(AuditDto.Action),
                DynamicFilterOperator.NotEquals,
                "Update")
        };

        var result = CreateSource()
            .ApplyDynamicFilters<AuditEntity, AuditDto>(filters, translator)
            .ToList();

        result.Select(x => x.Id).Should().Equal(2);
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