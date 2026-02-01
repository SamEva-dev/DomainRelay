namespace DomainRelay.EFCore.Outbox.Admin;

public interface IOutboxAdmin
{
    Task<OutboxStats> GetStatsAsync(CancellationToken ct);

    /// <summary>Returns the most recent dead-letter messages (IDs + metadata).</summary>
    Task<IReadOnlyList<OutboxMessage>> GetDeadLettersAsync(int take, CancellationToken ct);

    /// <summary>Requeue message(s) back to Pending state and eligible immediately.</summary>
    Task<int> RequeueAsync(IReadOnlyList<Guid> ids, bool resetAttempts, CancellationToken ct);

    /// <summary>Purge processed messages older than cutoff.</summary>
    Task<int> PurgeProcessedOlderThanAsync(DateTime cutoffUtc, CancellationToken ct);

    /// <summary>Hard delete by ids (dangerous; use for GDPR removal etc.).</summary>
    Task<int> DeleteByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken ct);
}
