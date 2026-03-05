using DomainRelay.Abstractions;

namespace DomainRelay.Internal;

internal abstract class NotificationHandlerWrapper
{
    public abstract Task Handle(IServiceProvider sp, object notification, CancellationToken ct);
}

internal sealed class NotificationHandlerWrapper<TNotification> : NotificationHandlerWrapper
    where TNotification : INotification
{
    public override Task Handle(IServiceProvider sp, object notification, CancellationToken ct)
        => HandleTyped(sp, (TNotification)notification, ct);

    private static Task HandleTyped(IServiceProvider sp, TNotification notification, CancellationToken ct)
    {
        var handlersObj = sp.GetService(typeof(IEnumerable<INotificationHandler<TNotification>>));
        var handlers = handlersObj as IEnumerable<INotificationHandler<TNotification>> ?? Array.Empty<INotificationHandler<TNotification>>();
        // Mediator will apply publish strategy; wrapper only exposes handlers list via service resolution.
        // Here we just execute sequentially if used directly (not used in current design).
        return Task.WhenAll(handlers.Select(h => h.Handle(notification, ct)));
    }
}
