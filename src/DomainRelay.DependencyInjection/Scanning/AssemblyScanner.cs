using System.Reflection;
using DomainRelay.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.DependencyInjection.Scanning;

internal static class AssemblyScanner
{
    public static void RegisterHandlers(IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        var allTypes = assemblies
            .Distinct()
            .SelectMany(a => SafeGetTypes(a))
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .ToArray();

        foreach (var impl in allTypes)
        {
            foreach (var itf in impl.GetInterfaces())
            {
                if (!itf.IsGenericType) continue;

                var def = itf.GetGenericTypeDefinition();

                if (def == typeof(IRequestHandler<,>) ||
                    def == typeof(INotificationHandler<>) ||
                    def == typeof(IPipelineBehavior<,>))
                {
                    services.AddTransient(itf, impl);
                }
            }
        }
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly a)
    {
        try { return a.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
    }
}
