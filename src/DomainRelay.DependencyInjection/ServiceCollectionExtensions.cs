using System.Reflection;
using DomainRelay.Abstractions;
using DomainRelay.DependencyInjection.Options;
using DomainRelay.DependencyInjection.Scanning;
using DomainRelay.Options;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.DependencyInjection;

/// <summary>
/// Dependency injection extensions for registering DomainRelay services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds DomainRelay mediator services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="configureOptions">Optional runtime options configuration.</param>
    /// <param name="configureRegistration">Optional registration and assembly scanning configuration.</param>
    /// <returns>The updated service collection.</returns>
    /// <remarks>
    /// Registers <see cref="IMediator"/> as scoped and optionally scans assemblies for handlers
    /// and pipeline behaviors.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddDomainRelay(
    ///     configureOptions: options =>
    ///     {
    ///         options.WrapExceptions = true;
    ///     },
    ///     configureRegistration: registration =>
    ///     {
    ///         registration.Assemblies.Add(typeof(CreateTenantCommandHandler).Assembly);
    ///     });
    /// </code>
    /// </example>
    public static IServiceCollection AddDomainRelay(
        this IServiceCollection services,
        Action<DomainRelayOptions>? configureOptions = null,
        Action<DomainRelayRegistrationOptions>? configureRegistration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new DomainRelayOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        var reg = new DomainRelayRegistrationOptions();
        configureRegistration?.Invoke(reg);

        if (reg.Assemblies.Count == 0)
            reg.Assemblies.Add(Assembly.GetCallingAssembly());

        services.AddScoped<IMediator, Mediator>();

        if (reg.EnableAssemblyScanning)
            AssemblyScanner.RegisterHandlers(services, reg.Assemblies);

        return services;
    }
}