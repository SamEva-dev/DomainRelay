# DomainRelay.Abstractions

Contracts used by DomainRelay (MediatR-style API surface):

- `IMediator`
- `IRequest<TResponse>` / `IRequestHandler<TRequest, TResponse>`
- `INotification` / `INotificationHandler<TNotification>`
- `IPipelineBehavior<TRequest, TResponse>`
- `Unit`

This package has no DI or runtime implementation. For the mediator implementation, install `DomainRelay` and `DomainRelay.DependencyInjection`.

## Requests (Commands/Queries)

```csharp
public sealed record CreateUser(string Email) : IRequest<Unit>;

public sealed class CreateUserHandler : IRequestHandler<CreateUser, Unit>
{
    public Task<Unit> Handle(CreateUser request, CancellationToken ct)
        => Task.FromResult(Unit.Value);
}
```

## Notifications (Domain Events)

```csharp
public sealed record UserCreated(Guid UserId) : INotification;

public sealed class SendWelcomeEmail : INotificationHandler<UserCreated>
{
    public Task Handle(UserCreated notification, CancellationToken ct)
        => Task.CompletedTask;
}
```

## Pipeline behaviors

```csharp
public sealed class TimingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(TRequest request, CancellationToken ct, HandlerDelegate<TResponse> next)
        => next();
}
```
