using DomainRelay.Abstractions;
using DomainRelay.Validation.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Validation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainRelayValidation(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(FluentValidationBehavior<,>));
        return services;
    }
}
