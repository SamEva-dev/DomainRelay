using System.Reflection;
using DomainRelay.Abstractions;
using DomainRelay.DependencyInjection.Options;
using DomainRelay.DependencyInjection.Scanning;
using DomainRelay.Options;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainRelay(
        this IServiceCollection services,
        Action<DomainRelayOptions>? configureOptions = null,
        Action<DomainRelayRegistrationOptions>? configureRegistration = null)
    {
        var options = new DomainRelayOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        var reg = new DomainRelayRegistrationOptions();
        configureRegistration?.Invoke(reg);

        // default: calling assembly
        if (reg.Assemblies.Count == 0)
            reg.Assemblies.Add(Assembly.GetCallingAssembly());

        services.AddScoped<IMediator, Mediator>();

        if (reg.EnableAssemblyScanning)
            AssemblyScanner.RegisterHandlers(services, reg.Assemblies);

        return services;
    }
}
