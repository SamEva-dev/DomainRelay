using DomainRelay.Mapping.Abstractions.Configuration;

namespace DomainRelay.Mapping.Abstractions.Profiles;

/// <summary>
/// Base class for grouping mapping configuration.
/// </summary>
/// <remarks>
/// Create one profile per bounded context, module or feature area, then register profiles through
/// dependency injection.
/// </remarks>
/// <example>
/// <code>
/// public sealed class TenantMappingProfile : MappingProfile
/// {
///     public override void Configure(IMappingConfiguration configuration)
///     {
///         configuration.CreateMap&lt;Tenant, TenantDto&gt;();
///     }
/// }
/// </code>
/// </example>
public abstract class MappingProfile
{
    /// <summary>
    /// Configures mappings for this profile.
    /// </summary>
    /// <param name="configuration">The mapping configuration to update.</param>
    public abstract void Configure(IMappingConfiguration configuration);
}