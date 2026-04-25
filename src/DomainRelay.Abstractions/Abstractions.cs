namespace DomainRelay.Abstractions;

/// <summary>
/// Marker interface for a command or request that does not return a value.
/// </summary>
/// <remarks>
/// Use <see cref="IRequest"/> for commands where the caller only needs to know
/// that the operation completed successfully.
/// </remarks>
/// <example>
/// <code>
/// public sealed record DeleteTenantCommand(Guid TenantId) : IRequest;
/// </code>
/// </example>
public interface IRequest
{
}

/// <summary>
/// Marker interface for a command or query returning a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">
/// The response type produced by the request handler.
/// </typeparam>
/// <remarks>
/// Use <see cref="IRequest{TResponse}"/> for queries or commands that return data.
/// </remarks>
/// <example>
/// <code>
/// public sealed record GetTenantQuery(Guid TenantId) : IRequest&lt;TenantDto&gt;;
/// </code>
/// </example>
public interface IRequest<out TResponse> : IRequest
{
}

/// <summary>
/// Handles a request that does not return a value.
/// </summary>
/// <typeparam name="TRequest">
/// The request type handled by this handler.
/// </typeparam>
/// <remarks>
/// Exactly one handler is expected for a request type.
/// </remarks>
/// <example>
/// <code>
/// public sealed class DeleteTenantCommandHandler : IRequestHandler&lt;DeleteTenantCommand&gt;
/// {
///     public Task Handle(DeleteTenantCommand request, CancellationToken ct)
///     {
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Handles the specified request.
    /// </summary>
    /// <param name="request">The request instance to handle.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(TRequest request, CancellationToken ct);
}

/// <summary>
/// Handles a request and returns a response.
/// </summary>
/// <typeparam name="TRequest">
/// The request type handled by this handler.
/// </typeparam>
/// <typeparam name="TResponse">
/// The response type returned by the handler.
/// </typeparam>
/// <remarks>
/// Exactly one handler is expected for a request type.
/// </remarks>
/// <example>
/// <code>
/// public sealed class GetTenantQueryHandler : IRequestHandler&lt;GetTenantQuery, TenantDto&gt;
/// {
///     public Task&lt;TenantDto&gt; Handle(GetTenantQuery request, CancellationToken ct)
///     {
///         return Task.FromResult(new TenantDto());
///     }
/// }
/// </code>
/// </example>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request and returns a response.
    /// </summary>
    /// <param name="request">The request instance to handle.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
    /// <returns>The response produced by the handler.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken ct);
}

/// <summary>
/// Marker interface for publishable notifications.
/// </summary>
/// <remarks>
/// Notifications are used for events that may have zero, one or many handlers.
/// They are commonly used for domain events or integration events.
/// </remarks>
/// <example>
/// <code>
/// public sealed record TenantCreatedNotification(Guid TenantId) : INotification;
/// </code>
/// </example>
public interface INotification
{
}

/// <summary>
/// Handles a notification of type <typeparamref name="TNotification"/>.
/// </summary>
/// <typeparam name="TNotification">
/// The notification type handled by this handler.
/// </typeparam>
/// <remarks>
/// Unlike requests, notifications may have multiple handlers.
/// </remarks>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles the specified notification.
    /// </summary>
    /// <param name="notification">The notification instance to handle.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(TNotification notification, CancellationToken ct);
}

/// <summary>
/// Represents the next handler step in a pipeline for a request without response.
/// </summary>
/// <returns>A task representing the asynchronous operation.</returns>
public delegate Task HandlerDelegate();

/// <summary>
/// Represents the next handler step in a pipeline for a request returning <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The response type returned by the pipeline.</typeparam>
/// <returns>The response produced by the next pipeline step.</returns>
public delegate Task<TResponse> HandlerDelegate<TResponse>();

/// <summary>
/// Defines a pipeline behavior around a request that does not return a value.
/// </summary>
/// <typeparam name="TRequest">
/// The request type handled by the pipeline behavior.
/// </typeparam>
/// <remarks>
/// Pipeline behaviors are useful for cross-cutting concerns such as validation,
/// logging, transactions, diagnostics or authorization.
/// </remarks>
public interface IPipelineBehavior<TRequest>
    where TRequest : IRequest
{
    /// <summary>
    /// Executes behavior logic before and/or after the next pipeline step.
    /// </summary>
    /// <param name="request">The request currently being handled.</param>
    /// <param name="next">The next pipeline step.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task Handle(TRequest request, HandlerDelegate next, CancellationToken ct);
}

/// <summary>
/// Defines a pipeline behavior around a request returning <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">
/// The request type handled by the pipeline behavior.
/// </typeparam>
/// <typeparam name="TResponse">
/// The response type returned by the request pipeline.
/// </typeparam>
/// <remarks>
/// Pipeline behaviors are executed in registration order and can call
/// <paramref name="next"/> to continue the request pipeline.
/// </remarks>
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Executes behavior logic before and/or after the next pipeline step.
    /// </summary>
    /// <param name="request">The request currently being handled.</param>
    /// <param name="next">The next pipeline step.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
    /// <returns>The response produced by the pipeline.</returns>
    Task<TResponse> Handle(TRequest request, HandlerDelegate<TResponse> next, CancellationToken ct);
}

/// <summary>
/// Main mediator contract used to send requests and publish notifications.
/// </summary>
/// <remarks>
/// <see cref="IMediator"/> decouples callers from request handlers and notification handlers.
/// Use <see cref="Send{TResponse}"/> for commands or queries and <see cref="Publish{TNotification}"/>
/// for notifications.
/// </remarks>
public interface IMediator
{
    /// <summary>
    /// Sends a request and returns its response.
    /// </summary>
    /// <typeparam name="TResponse">
    /// The response type expected from the request handler.
    /// </typeparam>
    /// <param name="request">The request instance to send.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
    /// <returns>The response returned by the matching request handler.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);

    /// <summary>
    /// Sends a request that does not return a value.
    /// </summary>
    /// <param name="request">The request instance to send.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    Task Send(IRequest request, CancellationToken ct = default);

    /// <summary>
    /// Publishes a notification to all registered handlers.
    /// </summary>
    /// <typeparam name="TNotification">
    /// The notification type to publish.
    /// </typeparam>
    /// <param name="notification">The notification instance to publish.</param>
    /// <param name="ct">A cancellation token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="notification"/> is <see langword="null"/>.
    /// </exception>
    Task Publish<TNotification>(TNotification notification, CancellationToken ct = default)
        where TNotification : INotification;
}