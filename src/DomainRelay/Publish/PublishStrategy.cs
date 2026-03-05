using DomainRelay.Abstractions;

namespace DomainRelay.Publish;

public interface IPublishStrategy
{
    Task Publish<TNotification>(
        IReadOnlyList<INotificationHandler<TNotification>> handlers,
        TNotification notification,
        CancellationToken ct)
        where TNotification : INotification;
}
