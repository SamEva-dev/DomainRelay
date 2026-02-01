public sealed record CreateUser(string Email) : IRequest<Unit>;

public sealed class CreateUserHandler : IRequestHandler<CreateUser, Unit>
{
  public Task<Unit> Handle(CreateUser request, CancellationToken ct)
  {
    // ...
    return Task.FromResult(Unit.Value);
  }
}


using DomainRelay.DependencyInjection;
using DomainRelay.Diagnostics;
using DomainRelay.EFCore;
using DomainRelay.Publish;
using DomainRelay.Validation;

services.AddDomainRelay(
    configureOptions: opt =>
    {
        opt.PublishStrategy = new ParallelPublishStrategy(); // ou Sequential (d�faut)
    },
    configureRegistration: reg =>
    {
        reg.Assemblies.Add(typeof(Program).Assembly);
        reg.Assemblies.Add(typeof(SomeApplicationHandler).Assembly);
    });

services.AddDomainRelayDiagnostics();
services.AddDomainRelayValidation();
services.AddDomainRelayEfCoreTransaction();


using DomainRelay.EFCore.DomainEvents;

public sealed record UserCreated(Guid UserId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
}

public sealed class User : IHasDomainEvents
{
    private readonly List<IDomainEvent> _events = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _events;

    public void ClearDomainEvents() => _events.Clear();

    public void RaiseCreated(Guid userId) => _events.Add(new UserCreated(userId));
}

//Outbox register
services.AddDomainRelayEfCoreOutbox<MyDbContext>(
  configureOptions: opt =>
  {
    opt.PollingInterval = TimeSpan.FromSeconds(2);
    opt.BatchSize = 100;
    opt.LeaseDuration = TimeSpan.FromSeconds(30);
    opt.MaxAttempts = 12;
    opt.ProcessedRetention = TimeSpan.FromDays(7);
  },
  configureRegistry: reg =>
  {
    reg.Register<UserCreated>();
    // register ALL event types explicitly (allowlist)
  });

Wire the interceptor into DbContext

When registering your DbContext, add the interceptor

services.AddDbContext<MyDbContext>((sp, o) =>
{
    o.UseNpgsql(connString); // or SqlServer, Sqlite, etc.
    o.AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>());
});

Provide a transport publisher

Implement IOutboxPublisher:
public sealed class MyBusPublisher : IOutboxPublisher
{
    public Task PublishAsync(OutboxEnvelope envelope, CancellationToken ct)
    {
        // Publish envelope.PayloadJson to your bus with envelope.TypeKey / EventId
        // Ensure idempotency downstream via EventId
        return Task.CompletedTask;
    }
}
Register it:
services.AddSingleton<IOutboxPublisher, MyBusPublisher>();

//EXEMPLE:

---

# K) Exemple d’intégration “clean” (recommandé)

Dans ton `Program.cs` :

```csharp
using DomainRelay.EFCore.Outbox;
using DomainRelay.EFCore.Outbox.Abstractions;

services.AddSingleton<IOutboxPublisher, MyBusPublisher>();

services.AddDomainRelayEfCoreOutbox<MyDbContext>(
    configureOptions: o =>
    {
        o.InstanceId = $"api-{Environment.MachineName}";
        o.BatchSize = 100;
        o.PollingInterval = TimeSpan.FromSeconds(1);
        o.LeaseDuration = TimeSpan.FromSeconds(30);
        o.MaxAttempts = 12;
        o.ProcessedRetention = TimeSpan.FromDays(14);
    },
    configureRegistry: reg =>
    {
        reg.Register<UserCreated>();
        // reg.Register<AnotherEvent>("custom.type.key");
    });

// DbContext + interceptor
services.AddDbContext<MyDbContext>((sp, o) =>
{
    o.UseNpgsql(builder.Configuration.GetConnectionString("db"));
    o.AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>());
});

// IMPORTANT: For dispatcher we also need a DbContextFactory configured
services.AddDbContextFactory<MyDbContext>((sp, o) =>
{
    o.UseNpgsql(builder.Configuration.GetConnectionString("db"));
    o.AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>());
});
