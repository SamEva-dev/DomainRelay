
### `src/DomainRelay.EFCore/README.md`
```md
# DomainRelay.EFCore

EF Core transaction pipeline behavior with single or multi-DbContext resolver.

```csharp
services.AddDomainRelayEfCoreTransactionResolver<MyDbContext>();


services.AddDbContext<AuthDbContext>(...);
services.AddDbContext<AppDbContext>(...);

services.AddDomainRelayEfCoreTransactionResolver(resolver =>
{
    resolver
      .Map<AuthDbContext>(t => t.FullName!.Contains(".Auth."))
      .Map<AppDbContext>(t => t.FullName!.Contains(".Application."));
});
