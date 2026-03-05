# DomainRelay.EFCore

EF Core integration for DomainRelay:

- Transaction pipeline behavior (single or multi-DbContext resolver)
- Premium Outbox (SaveChanges interceptor + background dispatcher + admin operations)

## Install

- `DomainRelay.EFCore`

## Transactions

### Single DbContext

```csharp
using DomainRelay.EFCore;

services.AddDomainRelayEfCoreTransactionResolver<MyDbContext>();
```

### Multi DbContext

```csharp
using DomainRelay.EFCore;

services.AddDbContext<AuthDbContext>(/* ... */);
services.AddDbContext<AppDbContext>(/* ... */);

services.AddDomainRelayEfCoreTransactionResolver(resolver =>
{
    resolver
        .Map<AuthDbContext>(t => t.FullName!.Contains(".Auth."))
        .Map<AppDbContext>(t => t.FullName!.Contains(".Application."));
});
```

## Outbox (recommended for production)

### 1) Add Outbox mapping

```csharp
using DomainRelay.EFCore.Outbox;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.AddDomainRelayOutbox(tableName: "outbox_messages", schema: "ops");
}
```

### 2) Register Outbox + Dispatcher

```csharp
using DomainRelay.EFCore;
using DomainRelay.EFCore.Outbox;
using DomainRelay.EFCore.Outbox.Abstractions;

services.AddSingleton<IOutboxPublisher, MyBusPublisher>();

services.AddDomainRelayEfCoreOutbox<MyDbContext>(b =>
{
    b.WithDbContextOptions((sp, o) =>
    {
        o.UseNpgsql(builder.Configuration.GetConnectionString("db"));
    });

    b.WithOutboxOptions(o =>
    {
        o.Schema = "ops";
        o.TableName = "outbox_messages";
        o.BatchSize = 200;
        o.PollingInterval = TimeSpan.FromSeconds(1);
        o.LeaseDuration = TimeSpan.FromSeconds(30);
        o.MaxAttempts = 12;
        o.ProcessedRetention = TimeSpan.FromDays(14);
    });

    // Allowlist registry (required): register all event types explicitly
    b.WithTypeRegistry(reg =>
    {
        reg.Register<UserCreated>("iam.user.created.v1");
    });
});
```

### 3) Operate (stats, dead-letter, requeue)

`DomainRelay.EFCore` registers `IOutboxAdmin` for operator actions:

- Stats/health: `GetStatsAsync()`
- Inspect dead-letters: `GetDeadLettersAsync(...)`
- Requeue: `RequeueAsync(...)`
- Purge processed: `PurgeProcessedOlderThanAsync(...)`
