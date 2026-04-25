using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Exceptions;
using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using DomainRelay.Mapping.Expressions.Extensions;
using DomainRelay.Mapping.Expressions.Queryable;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Mapping.Expressions.Tests;

public sealed class ExpressionTranslationExceptionTests
{
    [Fact]
    public void WhereTranslated_Should_Throw_ExpressionTranslationException_When_Destination_Member_Is_Not_Mapped()
    {
        using var provider = CreateProvider();

        var translator = provider.GetRequiredService<IExpressionTranslator>();

        var query = new[]
        {
            new AuditEntity { Id = Guid.NewGuid(), Action = "Create" }
        }.AsQueryable();

        var act = () => query
            .WhereTranslated<AuditEntity, AuditDto>(
                dto => dto.Unmapped == "x",
                translator)
            .ToList();

        act.Should()
            .Throw<ExpressionTranslationException>()
            .Where(ex => ex.SourceType == typeof(AuditEntity))
            .Where(ex => ex.DestinationType == typeof(AuditDto));
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();

        services.AddDomainRelayMapping(mapping =>
        {
            mapping
                .AddProfile<AuditProfile>()
                .ValidateConfigurationOnBuild();
        });

        services.AddDomainRelayMappingExpressions();

        return services.BuildServiceProvider();
    }

    private sealed class AuditProfile : MappingProfile
    {
        public override void Configure(IMappingConfiguration configuration)
        {
            configuration.CreateMap<AuditEntity, AuditDto>()
                .ForMember(d => d.Unmapped, opt => opt.Ignore());
        }
    }

    private sealed class AuditEntity
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
    }

    private sealed class AuditDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Unmapped { get; set; } = string.Empty;
    }
}