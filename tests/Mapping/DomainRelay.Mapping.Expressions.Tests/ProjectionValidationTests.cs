using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using DomainRelay.Mapping.Expressions.Extensions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DomainRelay.Mapping.Expressions.Tests;

public sealed class ProjectionValidationTests
{
    [Fact]
    public void BuildProjection_Should_Throw_ProjectionConfigurationException_When_Destination_Has_Unmapped_Ctor_Parameter()
    {
        using var provider = CreateProvider<InvalidRecordProfile>();

        var projectionBuilder = provider.GetRequiredService<IProjectionBuilder>();

        var act = () => projectionBuilder.BuildProjection<AuditEntity, InvalidAuditRecordDto>();

        act.Should()
            .Throw<ProjectionConfigurationException>()
            .Where(ex => ex.SourceType == typeof(AuditEntity))
            .Where(ex => ex.DestinationType == typeof(InvalidAuditRecordDto))
            .Where(ex => ex.Errors.Any(e => e.Contains("Missing", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void BuildProjection_Should_Throw_ProjectionConfigurationException_When_Member_Is_Not_Projectable()
    {
        using var provider = CreateProvider<InvalidMemberProfile>();

        var projectionBuilder = provider.GetRequiredService<IProjectionBuilder>();

        var act = () => projectionBuilder.BuildProjection<AuditEntity, InvalidAuditDto>();

        act.Should()
            .Throw<ProjectionConfigurationException>()
            .Where(ex => ex.SourceType == typeof(AuditEntity))
            .Where(ex => ex.DestinationType == typeof(InvalidAuditDto));
    }

    private static ServiceProvider CreateProvider<TProfile>()
        where TProfile : MappingProfile, new()
    {
        var services = new ServiceCollection();

        services.AddDomainRelayMapping(mapping =>
        {
            mapping
                .AddProfile<TProfile>()
                .ValidateConfigurationOnBuild();
        });

        services.AddDomainRelayMappingExpressions();

        return services.BuildServiceProvider();
    }

    private sealed class InvalidRecordProfile : MappingProfile
    {
        public override void Configure(IMappingConfiguration configuration)
        {
            configuration.CreateMap<AuditEntity, InvalidAuditRecordDto>();
        }
    }

    private sealed class InvalidMemberProfile : MappingProfile
    {
        public override void Configure(IMappingConfiguration configuration)
        {
            configuration.CreateMap<AuditEntity, InvalidAuditDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.StatusCode.ToString()));
        }
    }

    private sealed class AuditEntity
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }

    private sealed record InvalidAuditRecordDto(Guid Id, string Action, string Missing);

    private sealed class InvalidAuditDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}