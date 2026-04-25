using DomainRelay.Abstractions;

namespace DomainRelay.Publish;

/// <summary>
/// Defines how <see cref="ParallelPublishStrategy"/> handles handler failures.
/// </summary>
public enum ParallelPublishErrorMode
{
    /// <summary>
    /// Waits for all handlers to complete and throws an <see cref="AggregateException"/>
    /// when one or more handlers fail.
    /// </summary>
    WaitAllAggregate,

    /// <summary>
    /// Observes handler completion and rethrows the first failed handler exception.
    /// Remaining handlers may still continue running.
    /// </summary>
    FailFast
}

/// <summary>
/// Publishes notifications by invoking handlers concurrently.
/// </summary>
/// <remarks>
/// This strategy is useful when notification handlers are independent and can safely run in parallel.
/// Do not use this strategy when handlers depend on execution order or share non-thread-safe state.
/// </remarks>
public sealed class ParallelPublishStrategy : IPublishStrategy
{
    private readonly ParallelPublishErrorMode _mode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelPublishStrategy"/> class.
    /// </summary>
    /// <param name="mode">The error handling mode used when one or more handlers fail.</param>
    public ParallelPublishStrategy(ParallelPublishErrorMode mode = ParallelPublishErrorMode.WaitAllAggregate)
    {
        _mode = mode;
    }

    /// <inheritdoc />
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
            var remaining = tasks.ToList();
            while (remaining.Count > 0)
            {
                var finished = await Task.WhenAny(remaining).ConfigureAwait(false);
                remaining.Remove(finished);
                await finished.ConfigureAwait(false);
            }

            return;
        }

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