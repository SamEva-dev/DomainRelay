using DomainRelay.Abstractions;
using DomainRelay.Diagnostics.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Diagnostics;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainRelayDiagnostics(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DiagnosticsBehavior<,>));
        return services;
    }
}
