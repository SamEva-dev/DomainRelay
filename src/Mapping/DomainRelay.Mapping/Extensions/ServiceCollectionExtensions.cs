using DomainRelay.Mapping.Abstractions.Profiles;
using DomainRelay.Mapping.Abstractions.Services;
using DomainRelay.Mapping.Cache;
using DomainRelay.Mapping.Collections;
using DomainRelay.Mapping.Configuration;
using DomainRelay.Mapping.Engine;
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
        var builder = new DomainRelayMappingBuilder(services);
        configure?.Invoke(builder);

        var configuration = new MappingConfiguration();

        foreach (var profileType in builder.ProfileTypes.Distinct())
        {
            var profile = (MappingProfile)Activator.CreateInstance(profileType)!;
            profile.Configure(configuration);
        }

        services.AddSingleton(configuration);
        services.AddSingleton<TypeMapFactory>();
        services.AddSingleton<MappingPlanBuilder>();
        services.AddSingleton<MappingPlanCache>();
        services.AddSingleton<MappingValidator>();

        services.AddSingleton<ICollectionMapper, CollectionMapper>();
        services.AddSingleton<IDictionaryMapper, DictionaryMapper>();

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

        services.AddSingleton<IObjectMapper, ObjectMapper>();

        return services;
    }
}