# Getting Started

## Install
- DomainRelay.Abstractions
- DomainRelay
- DomainRelay.DependencyInjection

Optionals:
- DomainRelay.Diagnostics
- DomainRelay.Validation
- DomainRelay.EFCore

## Register
```csharp
services.AddDomainRelay(
  configureOptions: opt => {
    // opt.PublishStrategy = new ParallelPublishStrategy();
  },
  configureRegistration: reg => {
    reg.Assemblies.Add(typeof(SomeHandler).Assembly);
  });

services.AddDomainRelayDiagnostics();        // optional
services.AddDomainRelayValidation();         // optional (FluentValidation)
services.AddDomainRelayEfCoreTransaction();  // optional (EF Core)
