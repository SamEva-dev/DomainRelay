using DomainRelay.Abstractions;

namespace DomainRelay.Publish;

/// <summary>
/// Publishes notifications by invoking handlers sequentially.
/// </summary>
/// <remarks>
/// This is the default publish strategy. It preserves handler execution order according to
/// dependency injection registration order.
/// </remarks>
public sealed class SequentialPublishStrategy : IPublishStrategy
{
    /// <inheritdoc />
    public async Task Publish<TNotification>(
        IReadOnlyList<INotificationHandler<TNotification>> handlers,
        TNotification notification,
        CancellationToken ct)
        where TNotification : INotification
    {
        foreach (var h in handlers)
            await h.Handle(notification, ct).ConfigureAwait(false);
    }
}