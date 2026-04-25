using DomainRelay.Abstractions;
using DomainRelay.Diagnostics.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Diagnostics;

/// <summary>
/// Dependency injection extensions for DomainRelay diagnostics.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds DomainRelay diagnostics pipeline behavior.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The updated service collection.</returns>
    /// <remarks>
    /// This registers <see cref="DiagnosticsBehavior{TRequest,TResponse}"/> as an open generic
    /// pipeline behavior. It can be used to add tracing, timing and diagnostics around requests.
    /// </remarks>
    public static IServiceCollection AddDomainRelayDiagnostics(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DiagnosticsBehavior<,>));
        return services;
    }
}