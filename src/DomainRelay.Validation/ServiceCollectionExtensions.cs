using DomainRelay.Abstractions;
using DomainRelay.Validation.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Validation;

/// <summary>
/// Dependency injection extensions for DomainRelay validation.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds FluentValidation-based pipeline validation to DomainRelay.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <returns>The updated service collection.</returns>
    /// <remarks>
    /// This registers <see cref="FluentValidationBehavior{TRequest,TResponse}"/> as an open generic
    /// pipeline behavior. Validators must be registered separately through FluentValidation.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddValidatorsFromAssembly(typeof(CreateTenantCommandValidator).Assembly);
    /// services.AddDomainRelayValidation();
    /// </code>
    /// </example>
    public static IServiceCollection AddDomainRelayValidation(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FluentValidationBehavior<,>));
        return services;
    }
}