# DomainRelay.Transport.RabbitMQ

RabbitMQ publisher for DomainRelay EF Core Outbox.

- Implements `IOutboxPublisher`
- Topic routing via `IOutboxRouter` (default uses TypeKey as routing key)
- Supports: exchange declare, publisher confirms, headers, W3C tracing propagation

Usage:
```csharp
services.AddDomainRelayRabbitMqPublisher(o =>
{
  o.HostName = "rabbitmq";
  o.UserName = "guest";
  o.Password = "guest";
  o.ExchangeName = "domainrelay.events";
});


---

## ✅ Routing (transport-agnostic mais placé dans le package RabbitMQ)

### `src/DomainRelay.Transport.RabbitMQ/Routing/OutboxRoute.cs`
```csharp
namespace DomainRelay.Transport.RabbitMQ.Routing;

public sealed record OutboxRoute(
    string Exchange,
    string RoutingKey,
    bool Mandatory = false,
    bool Persistent = true
);
