using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Generation;
using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.Cache;
using DomainRelay.Mapping.Collections;
using DomainRelay.Mapping.Configuration;
using DomainRelay.Mapping.Diagnostics;
using DomainRelay.Mapping.Engine;
using DomainRelay.Mapping.Generation;
using DomainRelay.Mapping.Planning;
using DomainRelay.Mapping.Resolution;
using DomainRelay.Mapping.Resolution.Converters;
using DomainRelay.Mapping.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Mapping.DependencyInjection.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainRelayMapping(
        this IServiceCollection services,
        Action<DomainRelayMappingBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = new DomainRelayMappingBuilder(services);
        configure?.Invoke(builder);

        var configuration = new MappingConfiguration();

        foreach (var profileType in builder.ProfileTypes.Distinct())
        {
            var profile = (MappingProfile)Activator.CreateInstance(profileType)!;
            profile.Configure(configuration);
        }

        if (builder.ValidateConfigurationOnBuildEnabled)
        {
            configuration.AssertConfigurationIsValid();
        }

        services.AddSingleton(configuration);
        services.AddSingleton<IMappingConfiguration>(configuration);
        services.AddSingleton<IMapperConfigurationProvider>(configuration);

        RegisterGeneratedRegistryIfNeeded(services, builder);

        services.AddSingleton(new MappingRuntimeOptions());

        services.AddSingleton<TypeMapFactory>();
        services.AddSingleton<MappingPlanBuilder>();
        services.AddSingleton<MappingPlanCache>();
        services.AddSingleton<CompiledMappingPlanBuilder>();
        services.AddSingleton<CompiledMappingPlanCache>();
        services.AddSingleton<MemberAccessorCache>();
        services.AddSingleton<MappingValidator>();

        services.AddSingleton<ICollectionMapper, CollectionMapper>();
        services.AddSingleton<IDictionaryMapper, DictionaryMapper>();

        services.AddSingleton<IMappingDiagnosticsCollector, InMemoryMappingDiagnosticsCollector>();

        services.AddSingleton<TypeConverterRegistry>(_ =>
        {
            var registry = new TypeConverterRegistry();
            registry.Register(new ToStringTypeConverter());
            registry.Register(new NullableTypeConverter());
            registry.Register(new EnumByNameTypeConverter());
            registry.Register(new EnumToStringTypeConverter());
            registry.Register(new EnumToEnumTypeConverter());
            registry.Register(new NumberToEnumTypeConverter());
            return registry;
        });

        services.AddSingleton<IObjectMapper>(sp =>
            new ObjectMapper(
                sp.GetRequiredService<MappingConfiguration>(),
                sp.GetRequiredService<TypeMapFactory>(),
                sp.GetRequiredService<MappingPlanBuilder>(),
                sp.GetRequiredService<MappingPlanCache>(),
                sp.GetRequiredService<CompiledMappingPlanCache>(),
                sp.GetRequiredService<MappingValidator>(),
                sp.GetRequiredService<TypeConverterRegistry>(),
                sp.GetRequiredService<ICollectionMapper>(),
                sp.GetRequiredService<IDictionaryMapper>(),
                sp.GetRequiredService<MappingRuntimeOptions>(),
                sp.GetRequiredService<IMappingDiagnosticsCollector>(),
                sp.GetService<IGeneratedMappingRegistry>(),
                sp));

        return services;
    }

    private static void RegisterGeneratedRegistryIfNeeded(
        IServiceCollection services,
        DomainRelayMappingBuilder builder)
    {
        var registryTypes = builder.GeneratedRegistryTypes
            .Distinct()
            .ToArray();

        if (registryTypes.Length == 0)
        {
            return;
        }

        var registryInstances = registryTypes
            .Select(CreateGeneratedRegistryInstance)
            .ToArray();

        if (registryInstances.Length == 1)
        {
            services.AddSingleton(typeof(IGeneratedMappingRegistry), registryInstances[0]);
            return;
        }

        services.AddSingleton<IGeneratedMappingRegistry>(
            new CompositeGeneratedMappingRegistry(registryInstances));
    }

    private static IGeneratedMappingRegistry CreateGeneratedRegistryInstance(Type registryType)
    {
        var instance = Activator.CreateInstance(registryType, nonPublic: true);
        if (instance is not IGeneratedMappingRegistry registry)
        {
            throw new InvalidOperationException(
                $"Unable to instantiate generated mapping registry '{registryType.FullName}'.");
        }

        return registry;
    }
}