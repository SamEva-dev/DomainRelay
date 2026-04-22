using System.Reflection;
using DomainRelay.Mapping.Abstractions.Generation;
using DomainRelay.Mapping.Abstractions.Profiles;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.Mapping.DependencyInjection;

public sealed class DomainRelayMappingBuilder
{
    private readonly List<Type> _profileTypes = new();
    private readonly List<Type> _generatedRegistryTypes = new();

    public IServiceCollection Services { get; }

    internal DomainRelayMappingBuilder(IServiceCollection services)
    {
        Services = services;
    }

    internal IReadOnlyList<Type> ProfileTypes => _profileTypes;
    internal IReadOnlyList<Type> GeneratedRegistryTypes => _generatedRegistryTypes;
    internal bool ValidateConfigurationOnBuildEnabled { get; private set; }

    public DomainRelayMappingBuilder AddProfile<TProfile>()
        where TProfile : MappingProfile
    {
        _profileTypes.Add(typeof(TProfile));
        return this;
    }

    public DomainRelayMappingBuilder AddProfile(Type profileType)
    {
        ArgumentNullException.ThrowIfNull(profileType);

        if (!typeof(MappingProfile).IsAssignableFrom(profileType) || profileType.IsAbstract)
        {
            throw new ArgumentException(
                $"The provided type '{profileType.FullName}' must be a non-abstract MappingProfile.",
                nameof(profileType));
        }

        _profileTypes.Add(profileType);
        return this;
    }

    public DomainRelayMappingBuilder AddProfile(MappingProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        _profileTypes.Add(profile.GetType());
        return this;
    }

    public DomainRelayMappingBuilder AddProfilesFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var profileTypes = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(MappingProfile).IsAssignableFrom(t));

        _profileTypes.AddRange(profileTypes);
        return this;
    }

    public DomainRelayMappingBuilder AddProfilesFromAssemblyContaining<TMarker>()
    {
        return AddProfilesFromAssembly(typeof(TMarker).Assembly);
    }

    public DomainRelayMappingBuilder AddGeneratedMappingRegistry<TRegistry>()
        where TRegistry : class, IGeneratedMappingRegistry
    {
        _generatedRegistryTypes.Add(typeof(TRegistry));
        return this;
    }

    public DomainRelayMappingBuilder AddGeneratedMappingRegistry(Type registryType)
    {
        ArgumentNullException.ThrowIfNull(registryType);

        if (!typeof(IGeneratedMappingRegistry).IsAssignableFrom(registryType) || registryType.IsAbstract)
        {
            throw new ArgumentException(
                $"The provided type '{registryType.FullName}' must implement IGeneratedMappingRegistry and be non-abstract.",
                nameof(registryType));
        }

        _generatedRegistryTypes.Add(registryType);
        return this;
    }

    public DomainRelayMappingBuilder AddGeneratedMappingsFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var registryTypes = assembly
            .GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                typeof(IGeneratedMappingRegistry).IsAssignableFrom(t) &&
                t.Namespace == "DomainRelay.Mapping.Generated");

        _generatedRegistryTypes.AddRange(registryTypes);
        return this;
    }

    public DomainRelayMappingBuilder AddGeneratedMappingsFromAssemblyContaining<TMarker>()
    {
        return AddGeneratedMappingsFromAssembly(typeof(TMarker).Assembly);
    }

    public DomainRelayMappingBuilder ValidateConfigurationOnBuild()
    {
        ValidateConfigurationOnBuildEnabled = true;
        return this;
    }
}