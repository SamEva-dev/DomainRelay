using DomainRelay.Mapping.Abstractions.Projection;
using DomainRelay.Mapping.Expressions.Projection;
using DomainRelay.Mapping.Expressions.Translation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DomainRelay.Mapping.Expressions.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainRelayMappingExpressions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<ProjectionPlanBuilder>();
        services.TryAddSingleton<ProjectionValidator>();
        services.TryAddSingleton<IProjectionBuilder, ProjectionBuilder>();

        services.TryAddSingleton<ExpressionTranslationPlanBuilder>();
        services.TryAddSingleton<ExpressionTranslationValidator>();
        services.TryAddSingleton<IExpressionTranslator, DestinationToSourceExpressionTranslator>();

        return services;
    }
}