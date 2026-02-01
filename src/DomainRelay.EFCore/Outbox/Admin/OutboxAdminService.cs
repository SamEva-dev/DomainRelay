using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DomainRelay.EFCore.Outbox.Admin;

public sealed class OutboxAdminService<TDbContext> : IOutboxAdmin
    where TDbContext : DbContext
{
    private readonly IDbContextFactory<TDbContext> _dbFactory;
    private readonly OutboxOptions _outboxOptions;
    private readonly OutboxAdminOptions _adminOptions;
    private readonly ILogger<OutboxAdminService<TDbContext>> _logger;

    public OutboxAdminService(
        IDbContextFactory<TDbContext> dbFactory,
        OutboxOptions outboxOptions,
        OutboxAdminOptions adminOptions,
        ILogger<OutboxAdminService<TDbContext>> logger)
    {
        _dbFactory = dbFactory;
        _outboxOptions = outboxOptions;
        _adminOptions = adminOptions;
        _logger = logger;
    }

    public async Task<OutboxStats> GetStatsAsync(CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var q = db.Set<OutboxMessage>().AsNoTracking();

        var pending = await q.LongCountAsync(x => x.Status == OutboxStatus.Pending, ct).ConfigureAwait(false);
        var processing = await q.LongCountAsync(x => x.Status == OutboxStatus.Processing, ct).ConfigureAwait(false);
        var processed = await q.LongCountAsync(x => x.Status == OutboxStatus.Processed, ct).ConfigureAwait(false);
        var dead = await q.LongCountAsync(x => x.Status == OutboxStatus.DeadLetter, ct).ConfigureAwait(false);
        var total = pending + processing + processed + dead;

        return new OutboxStats(pending, processing, processed, dead, total);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetDeadLettersAsync(int take, CancellationToken ct)
    {
        take = Math.Clamp(take, 1, 500);

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        return await db.Set<OutboxMessage>()
            .AsNoTracking()
            .Where(x => x.Status == OutboxStatus.DeadLetter)
            .OrderByDescending(x => x.EnqueuedAtUtc)
            .Take(take)
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }

    public async Task<int> RequeueAsync(IReadOnlyList<Guid> ids, bool resetAttempts, CancellationToken ct)
    {
        if (ids is null || ids.Count == 0) return 0;
        if (ids.Count > _adminOptions.MaxBulkOperationSize)
            throw new InvalidOperationException($"Too many ids. Max is {_adminOptions.MaxBulkOperationSize}.");

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var now = DateTime.UtcNow;

        var messages = await db.Set<OutboxMessage>()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct)
            .ConfigureAwait(false);

        foreach (var m in messages)
        {
            m.Status = OutboxStatus.Pending;
            m.NextAttemptUtc = now;
            m.LockedBy = null;
            m.LockedUntilUtc = null;
            m.LastError = null;

            if (resetAttempts)
                m.AttemptCount = 0;
        }

        var affected = await db.SaveChangesAsync(ct).ConfigureAwait(false);

        _logger.LogInformation("Outbox requeued {Count} messages (resetAttempts={Reset}).", messages.Count, resetAttempts);

        return affected;
    }

    public async Task<int> PurgeProcessedOlderThanAsync(DateTime cutoffUtc, CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        // safer multi-provider approach: delete by ids in batches
        var ids = await db.Set<OutboxMessage>()
            .Where(m => m.Status == OutboxStatus.Processed && m.ProcessedAtUtc != null && m.ProcessedAtUtc < cutoffUtc)
            .OrderBy(m => m.ProcessedAtUtc)
            .Select(m => m.Id)
            .Take(_adminOptions.MaxBulkOperationSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (ids.Count == 0) return 0;

        db.RemoveRange(ids.Select(id => new OutboxMessage { Id = id }));

        var affected = await db.SaveChangesAsync(ct).ConfigureAwait(false);

        _logger.LogInformation("Outbox purged {Count} processed messages older than {Cutoff}.", ids.Count, cutoffUtc);

        return affected;
    }

    public async Task<int> DeleteByIdsAsync(IReadOnlyList<Guid> ids, CancellationToken ct)
    {
        if (ids is null || ids.Count == 0) return 0;
        if (ids.Count > _adminOptions.MaxBulkOperationSize)
            throw new InvalidOperationException($"Too many ids. Max is {_adminOptions.MaxBulkOperationSize}.");

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        db.RemoveRange(ids.Select(id => new OutboxMessage { Id = id }));
        var affected = await db.SaveChangesAsync(ct).ConfigureAwait(false);

        _logger.LogWarning("Outbox hard deleted {Count} messages.", ids.Count);
        return affected;
    }
}
