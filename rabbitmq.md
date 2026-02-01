# RabbitMQ Transport (DomainRelay.Transport.RabbitMQ)

## Install
- DomainRelay.EFCore
- DomainRelay.Transport.RabbitMQ

## Register
```csharp
services.AddSingleton<IOutboxPublisher, RabbitMqOutboxPublisher>(); // via extension below

services.AddDomainRelayRabbitMqPublisher(o =>
{
  o.HostName = "rabbitmq";
  o.Port = 5672;
  o.VirtualHost = "/";
  o.UserName = "guest";
  o.Password = "guest";

  o.ExchangeName = "domainrelay.events";
  o.ExchangeType = "topic";
  o.DeclareExchange = true;
  o.ExchangeDurable = true;

  o.PublisherConfirms = true;
  o.ConfirmsTimeout = TimeSpan.FromSeconds(5);
});



---

# 3) Comment ça reste compatible “avec tout le reste”

- `DomainRelay.EFCore` ne connaît que `IOutboxPublisher`.
- RabbitMQ n’est qu’**une implémentation** de ce contrat.
- Tu peux remplacer à chaud par :
  - `KafkaOutboxPublisher : IOutboxPublisher`
  - `HttpWebhookOutboxPublisher : IOutboxPublisher`
  - etc.
- Le routing (`IOutboxRouter`) est **optionnel** : chaque transport peut l’ignorer ou le réutiliser.

---

# 4) Exemple d’intégration complète (Outbox premium + RabbitMQ)

```csharp
// 1) Transport
services.AddDomainRelayRabbitMqPublisher(o =>
{
    o.HostName = "rabbitmq";
    o.ExchangeName = "locaguest.events";
    o.ExchangeType = "topic";
    o.PublisherConfirms = true;
});

// 2) Outbox premium (capture + dispatcher + admin)
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
        o.InstanceId = $"api-{Environment.MachineName}";
        o.BatchSize = 200;
        o.PollingInterval = TimeSpan.FromSeconds(1);
        o.LeaseDuration = TimeSpan.FromSeconds(30);
        o.MaxAttempts = 12;
        o.ProcessedRetention = TimeSpan.FromDays(14);
    });

    b.WithTypeRegistry(reg =>
    {
        reg.Register<UserCreated>("iam.user.created.v1");
        reg.Register<UserDeleted>("iam.user.deleted.v1");
    });
});


---

# 3) Comment ça reste compatible “avec tout le reste”

- `DomainRelay.EFCore` ne connaît que `IOutboxPublisher`.
- RabbitMQ n’est qu’**une implémentation** de ce contrat.
- Tu peux remplacer à chaud par :
  - `KafkaOutboxPublisher : IOutboxPublisher`
  - `HttpWebhookOutboxPublisher : IOutboxPublisher`
  - etc.
- Le routing (`IOutboxRouter`) est **optionnel** : chaque transport peut l’ignorer ou le réutiliser.

---

# 4) Exemple d’intégration complète (Outbox premium + RabbitMQ)

```csharp
// 1) Transport
services.AddDomainRelayRabbitMqPublisher(o =>
{
    o.HostName = "rabbitmq";
    o.ExchangeName = "locaguest.events";
    o.ExchangeType = "topic";
    o.PublisherConfirms = true;
});

// 2) Outbox premium (capture + dispatcher + admin)
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
        o.InstanceId = $"api-{Environment.MachineName}";
        o.BatchSize = 200;
        o.PollingInterval = TimeSpan.FromSeconds(1);
        o.LeaseDuration = TimeSpan.FromSeconds(30);
        o.MaxAttempts = 12;
        o.ProcessedRetention = TimeSpan.FromDays(14);
    });

    b.WithTypeRegistry(reg =>
    {
        reg.Register<UserCreated>("iam.user.created.v1");
        reg.Register<UserDeleted>("iam.user.deleted.v1");
    });
});
