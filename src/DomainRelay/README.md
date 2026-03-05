# DomainRelay

Core mediator implementation for Clean Architecture.

## Install

- `DomainRelay.Abstractions`
- `DomainRelay`
- `DomainRelay.DependencyInjection`

Optionals:

- `DomainRelay.Diagnostics`
- `DomainRelay.Validation`
- `DomainRelay.EFCore`
- `DomainRelay.Transport.RabbitMQ`

## Quick start

Register DomainRelay and scan assemblies containing your handlers:

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
        reg.Assemblies.Add(typeof(Program).Assembly);
        // reg.Assemblies.Add(typeof(SomeHandler).Assembly);
    });
```

## Send (Commands / Queries)

```csharp
using DomainRelay.Abstractions;

public sealed record CreateUser(string Email) : IRequest<Unit>;

public sealed class CreateUserHandler : IRequestHandler<CreateUser, Unit>
{
    public Task<Unit> Handle(CreateUser request, CancellationToken ct)
        => Task.FromResult(Unit.Value);
}

await mediator.Send(new CreateUser("a@b.com"), ct);
```

## Publish (Notifications)

```csharp
using DomainRelay.Abstractions;

public sealed record UserCreated(Guid UserId) : INotification;

public sealed class SendWelcomeEmail : INotificationHandler<UserCreated>
{
    public Task Handle(UserCreated notification, CancellationToken ct) => Task.CompletedTask;
}

await mediator.Publish(new UserCreated(userId), ct);
```

## Pipeline behaviors

Behaviors wrap request handlers (validation, transactions, diagnostics, etc.):

```csharp
using DomainRelay.Abstractions;

public sealed class TimingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken ct, HandlerDelegate<TResponse> next)
        => await next();
}

services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TimingBehavior<,>));
```

## Related packages

- `DomainRelay.DependencyInjection` for `AddDomainRelay(...)`
- `DomainRelay.Diagnostics` for `ActivitySource` tracing
- `DomainRelay.Validation` for FluentValidation integration
- `DomainRelay.EFCore` for transactions + Outbox
