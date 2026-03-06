# DomainRelay.DependencyInjection

DI integration for DomainRelay.

## Install

- `DomainRelay.Abstractions`
- `DomainRelay`
- `DomainRelay.DependencyInjection`

## Register

```csharp
using DomainRelay.DependencyInjection;
using DomainRelay.Publish;

services.AddDomainRelay(
    configureOptions: o =>
    {
        // Notifications publish strategy (default: SequentialPublishStrategy)
        // o.PublishStrategy = new ParallelPublishStrategy();
    },
    configureRegistration: reg =>
    {
        // Assemblies containing IRequestHandler / INotificationHandler / IPipelineBehavior
        reg.Assemblies.Add(typeof(SomeHandler).Assembly);
    }
);
```

## Recommended registration (ASP.NET Core)

If your application contains open-generic handlers/behaviors (for example an `AuditBehavior<TRequest, TResponse>`),
you may prefer to disable the built-in assembly scanning and register handlers yourself.
This gives you full control over how open-generic implementations are mapped to open-generic service types.

```csharp
using System.Reflection;
using DomainRelay.Abstractions;
using DomainRelay.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class DomainRelaySetup
{
    public static IServiceCollection AddDomainRelayApp(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddDomainRelay(
            configureOptions: _ => { },
            configureRegistration: reg =>
            {
                // We still pass assemblies (for future extensions), but disable built-in scanning.
                foreach (var a in assemblies)
                    reg.Assemblies.Add(a);

                // Recommended when you want deterministic registration of open-generics.
                reg.EnableAssemblyScanning = false;
            });

        RegisterDomainRelayHandlers(services, assemblies);
        return services;
    }

    private static void RegisterDomainRelayHandlers(IServiceCollection services, params Assembly[] assemblies)
    {
        // Scan concrete types.
        var allTypes = assemblies
            .Distinct()
            .SelectMany(SafeGetTypes)
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .ToArray();

        foreach (var impl in allTypes)
        {
            foreach (var itf in impl.GetInterfaces())
            {
                if (!itf.IsGenericType) continue;

                var def = itf.GetGenericTypeDefinition();
                if (def != typeof(IRequestHandler<,>) &&
                    def != typeof(INotificationHandler<>) &&
                    def != typeof(IPipelineBehavior<,>))
                {
                    continue;
                }

                // Closed generic interface -> register interface as-is.
                if (!itf.ContainsGenericParameters)
                {
                    services.TryAddEnumerable(ServiceDescriptor.Transient(itf, impl));
                    continue;
                }

                // Open-generic interface (e.g. IPipelineBehavior<,>) ->
                // register the open-generic service type to the open-generic implementation.
                if (!impl.IsGenericTypeDefinition)
                    continue;

                services.TryAddEnumerable(ServiceDescriptor.Transient(def, impl));
            }
        }
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly a)
    {
        try { return a.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!.Cast<Type>(); }
    }
}
```

## Pipeline behaviors

You can register open-generic behaviors manually:

```csharp
services.AddTransient(typeof(DomainRelay.Abstractions.IPipelineBehavior<,>), typeof(MyBehavior<,>));
```

## Notes

- Assembly scanning uses DI enumerable registration patterns to avoid common duplicates when the same types are registered multiple times.
