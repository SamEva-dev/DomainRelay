# DomainRelay.DependencyInjection

DI integration for DomainRelay.

```csharp
services.AddDomainRelay(
  configureOptions: opt => { },
  configureRegistration: reg => reg.Assemblies.Add(typeof(SomeHandler).Assembly)
);
