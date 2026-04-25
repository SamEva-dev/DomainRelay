using DomainRelay.Abstractions;

namespace DomainRelay.Publish;

/// <summary>
/// Defines how DomainRelay publishes notifications to their handlers.
/// </summary>
/// <remarks>
/// Implement this interface to customize notification dispatching, for example sequential,
/// parallel, ordered, resilient or outbox-backed publishing.
/// </remarks>
public interface IPublishStrategy
{
    /// <summary>
    /// Publishes a notification to the specified handlers.
    /// </summary>
    /// <typeparam name="TNotification">The notification type.</typeparam>
    /// <param name="handlers">The notification handlers to invoke.</param>
    /// <param name="notification">The notification instance to publish.</param>
    /// <param name="ct">A cancellation token used to cancel the publish operation.</param>
    /// <returns>A task representing the asynchronous publish operation.</returns>
    Task Publish<TNotification>(
        IReadOnlyList<INotificationHandler<TNotification>> handlers,
        TNotification notification,
        CancellationToken ct)
        where TNotification : INotification;
}