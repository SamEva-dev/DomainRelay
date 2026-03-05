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

## Pipeline behaviors

You can register open-generic behaviors manually:

```csharp
services.AddTransient(typeof(DomainRelay.Abstractions.IPipelineBehavior<,>), typeof(MyBehavior<,>));
```

## Notes

- Assembly scanning uses DI enumerable registration patterns to avoid common duplicates when the same types are registered multiple times.
