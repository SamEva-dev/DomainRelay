using DomainRelay.Abstractions;

namespace DomainRelay.Publish;

public sealed class SequentialPublishStrategy : IPublishStrategy
{
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
