using System.Reflection;

namespace DomainRelay.DependencyInjection.Options;

/// <summary>
/// Options controlling handler and pipeline registration for DomainRelay.
/// </summary>
/// <remarks>
/// These options are configured through <c>AddDomainRelay</c>.
/// </remarks>
/// <example>
/// <code>
/// services.AddDomainRelay(
///     configureRegistration: registration =>
///     {
///         registration.Assemblies.Add(typeof(CreateTenantCommandHandler).Assembly);
///         registration.EnableAssemblyScanning = true;
///     });
/// </code>
/// </example>
public sealed class DomainRelayRegistrationOptions
{
    /// <summary>
    /// Gets the assemblies scanned for request handlers, notification handlers and pipeline behaviors.
    /// </summary>
    /// <remarks>
    /// When the collection is empty, the calling assembly is used by default.
    /// </remarks>
    public List<Assembly> Assemblies { get; } = new();

    /// <summary>
    /// Gets or sets whether DomainRelay should scan the configured assemblies and register handlers automatically.
    /// </summary>
    /// <remarks>
    /// When set to <see langword="false"/>, handlers and pipeline behaviors must be registered manually.
    /// </remarks>
    public bool EnableAssemblyScanning { get; set; } = true;
}