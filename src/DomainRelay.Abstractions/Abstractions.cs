namespace DomainRelay.Abstractions;

/// <summary>
/// Marker interface for a request returning <typeparamref name="TResponse"/>.
/// </summary>
public interface IRequest<out TResponse> { }

/// <summary>
/// Handles a request of type <typeparamref name="TRequest"/>.
/// </summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}

/// <summary>
/// Marker interface for publishable notifications (domain events).
/// </summary>
public interface INotification { }

/// <summary>
/// Handles a notification of type <typeparamref name="TNotification"/>.
/// Multiple handlers are allowed.
/// </summary>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken ct);
}

/// <summary>
/// Delegate representing the next step in a pipeline.
/// </summary>
public delegate Task<TResponse> HandlerDelegate<TResponse>();

/// <summary>
/// Pipeline behavior around request handling.
/// </summary>
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken ct, HandlerDelegate<TResponse> next);
}

/// <summary>
/// Main mediator contract.
/// </summary>
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);

    Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
        where TNotification : INotification;
}
