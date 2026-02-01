using System.Reflection;

namespace DomainRelay.DependencyInjection.Options;

public sealed class DomainRelayRegistrationOptions
{
    public List<Assembly> Assemblies { get; } = new();

    /// <summary>
    /// When true, scan types in assemblies to register IRequestHandler, INotificationHandler, IPipelineBehavior.
    /// </summary>
    public bool EnableAssemblyScanning { get; set; } = true;
}
