using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.Expressions.Projection;
using DomainRelay.Mapping.Expressions.Translation;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Mapping.Expressions.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainRelayMappingExpressions(this IServiceCollection services)
    {
        services.AddSingleton<ProjectionPlanBuilder>();
        services.AddSingleton<ProjectionValidator>();
        services.AddSingleton<IProjectionBuilder, ProjectionBuilder>();

        services.AddSingleton<ExpressionTranslationPlanBuilder>();
        services.AddSingleton<ExpressionTranslationValidator>();
        services.AddSingleton<IExpressionTranslator, DestinationToSourceExpressionTranslator>();

        return services;
    }
}