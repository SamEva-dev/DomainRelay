using DomainRelay.Abstractions;

namespace DomainRelay.Publish;

public enum ParallelPublishErrorMode
{
    /// <summary>Wait for all handlers; throw AggregateException if any failed.</summary>
    WaitAllAggregate,

    /// <summary>Fail fast (first exception). Remaining tasks may still run.</summary>
    FailFast
}

public sealed class ParallelPublishStrategy : IPublishStrategy
{
    private readonly ParallelPublishErrorMode _mode;

    public ParallelPublishStrategy(ParallelPublishErrorMode mode = ParallelPublishErrorMode.WaitAllAggregate)
    {
        _mode = mode;
    }

    public async Task Publish<TNotification>(
        IReadOnlyList<INotificationHandler<TNotification>> handlers,
        TNotification notification,
        CancellationToken ct)
        where TNotification : INotification
    {
        if (handlers.Count == 0) return;

        var tasks = handlers.Select(h => h.Handle(notification, ct)).ToArray();

        if (_mode == ParallelPublishErrorMode.FailFast)
        {
            // Fail-fast: await tasks in completion order
            var remaining = tasks.ToList();
            while (remaining.Count > 0)
            {
                var finished = await Task.WhenAny(remaining).ConfigureAwait(false);
                remaining.Remove(finished);
                await finished.ConfigureAwait(false); // throws if failed
            }
            return;
        }

        // WaitAllAggregate
        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch
        {
            var exceptions = tasks
                .Where(t => t.IsFaulted && t.Exception is not null)
                .SelectMany(t => t.Exception!.InnerExceptions)
                .ToList();

            throw new AggregateException("One or more notification handlers failed.", exceptions);
        }
    }
}
