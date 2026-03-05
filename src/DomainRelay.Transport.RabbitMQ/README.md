# DomainRelay.Transport.RabbitMQ

RabbitMQ publisher for DomainRelay EF Core Outbox.

- Implements `IOutboxPublisher`
- Topic routing via `IOutboxRouter` (default uses TypeKey as routing key)
- Supports: exchange declare, publisher confirms, headers, W3C tracing propagation

## Install

- `DomainRelay.EFCore`
- `DomainRelay.Transport.RabbitMQ`

Usage:
```csharp
using DomainRelay.Transport.RabbitMQ;

services.AddDomainRelayRabbitMqPublisher(o =>
{
  o.HostName = "rabbitmq";
  o.UserName = "guest";
  o.Password = "guest";
  o.ExchangeName = "domainrelay.events";
});
```

## Full integration (Outbox + RabbitMQ)

```csharp
using DomainRelay.EFCore;
using DomainRelay.EFCore.Outbox;
using DomainRelay.Transport.RabbitMQ;

services.AddDomainRelayRabbitMqPublisher(o =>
{
    o.HostName = "rabbitmq";
    o.ExchangeName = "locaguest.events";
    o.ExchangeType = "topic";
    o.PublisherConfirms = true;
});

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

    b.WithTypeRegistry(reg =>
    {
        reg.Register<UserCreated>("iam.user.created.v1");
    });
});
```


---

## Routing

### `src/DomainRelay.Transport.RabbitMQ/Routing/OutboxRoute.cs`
```csharp
namespace DomainRelay.Transport.RabbitMQ.Routing;

public sealed record OutboxRoute(
    string Exchange,
    string RoutingKey,
    bool Mandatory = false,
    bool Persistent = true
);
```
