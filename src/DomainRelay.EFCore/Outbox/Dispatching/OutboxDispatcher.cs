using DomainRelay.EFCore.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DomainRelay.EFCore.Outbox.Dispatching;

/// <summary>
/// Executes outbox dispatch loop logic (claim -> publish -> finalize).
/// Provider-agnostic using optimistic concurrency.
/// </summary>
public sealed class OutboxDispatcher<TDbContext>
    where TDbContext : DbContext
{
    private readonly IDbContextFactory<TDbContext> _dbFactory;
    private readonly IOutboxPublisher _publisher;
    private readonly IOutboxTypeRegistry _typeRegistry;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxDispatcher<TDbContext>> _logger;

    public OutboxDispatcher(
        IDbContextFactory<TDbContext> dbFactory,
        IOutboxPublisher publisher,
        IOutboxTypeRegistry typeRegistry,
        OutboxOptions options,
        ILogger<OutboxDispatcher<TDbContext>> logger)
    {
        _dbFactory = dbFactory;
        _publisher = publisher;
        _typeRegistry = typeRegistry;
        _options = options;
        _logger = logger;
    }

    public async Task<int> DispatchOnceAsync(CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

        var now = DateTime.UtcNow;
        var leaseUntil = now.Add(_options.LeaseDuration);

        // 1) Find candidates
        var candidates = await db.Set<OutboxMessage>()
            .Where(m =>
                (m.Status == OutboxStatus.Pending || m.Status == OutboxStatus.Processing) &&
                m.NextAttemptUtc <= now &&
                (m.LockedUntilUtc == null || m.LockedUntilUtc < now))
            .OrderBy(m => m.OccurredOnUtc)
            .ThenBy(m => m.EnqueuedAtUtc)
            .Take(_options.BatchSize)
            .AsNoTracking()
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (candidates.Count == 0)
            return 0;

        // 2) Claim with optimistic concurrency (attach + set lock + SaveChanges)
        var claimed = new List<OutboxMessage>(candidates.Count);

        foreach (var c in candidates)
        {
            var stub = new OutboxMessage
            {
                Id = c.Id,
                RowVersion = c.RowVersion
            };

            db.Attach(stub);

            stub.Status = OutboxStatus.Processing;
            stub.LockedBy = _options.InstanceId;
            stub.LockedUntilUtc = leaseUntil;

            try
            {
                await db.SaveChangesAsync(ct).ConfigureAwait(false);
                claimed.Add(c);
            }
            catch (DbUpdateConcurrencyException)
            {
                // someone else claimed it
                db.Entry(stub).State = EntityState.Detached;
            }
        }

        if (claimed.Count == 0)
            return 0;

        _logger.LogDebug("Outbox claimed {Count} messages (instance={InstanceId}).", claimed.Count, _options.InstanceId);

        // 3) Process claimed messages
        var processed = 0;

        foreach (var c in claimed)
        {
            ct.ThrowIfCancellationRequested();

            await using var db2 = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var msg = await db2.Set<OutboxMessage>().FirstOrDefaultAsync(x => x.Id == c.Id, ct).ConfigureAwait(false);
            if (msg is null)
                continue;

            // Ensure we still own lease
            if (!string.Equals(msg.LockedBy, _options.InstanceId, StringComparison.Ordinal) ||
                (msg.LockedUntilUtc is null) || msg.LockedUntilUtc < DateTime.UtcNow)
            {
                continue;
            }

            try
            {
                // Validate type key exists (safety)
                if (!_typeRegistry.TryResolve(msg.TypeKey, out _))
                {
                    await MarkDeadLetterAsync(db2, msg, "TypeKey is not registered in OutboxTypeRegistry.", ct).ConfigureAwait(false);
                    continue;
                }

                var env = new OutboxEnvelope(
                    OutboxId: msg.Id,
                    EventId: msg.EventId,
                    TypeKey: msg.TypeKey,
                    Version: msg.Version,
                    OccurredOnUtc: msg.OccurredOnUtc,
                    ContentType: msg.ContentType,
                    PayloadJson: msg.PayloadJson,
                    HeadersJson: msg.HeadersJson,
                    CorrelationId: msg.CorrelationId);

                await _publisher.PublishAsync(env, ct).ConfigureAwait(false);

                msg.Status = OutboxStatus.Processed;
                msg.ProcessedAtUtc = DateTime.UtcNow;
                msg.LockedUntilUtc = null;
                msg.LockedBy = null;
                msg.LastError = null;

                await db2.SaveChangesAsync(ct).ConfigureAwait(false);
                processed++;
            }
            catch (Exception ex)
            {
                await HandleFailureAsync(db2, msg, ex, ct).ConfigureAwait(false);
            }
        }

        return processed;
    }

    public async Task<int> CleanupAsync(CancellationToken ct)
    {
        if (_options.ProcessedRetention is null)
            return 0;

        await using var db = await _dbFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        var cutoff = DateTime.UtcNow.Subtract(_options.ProcessedRetention.Value);

        // Provider-agnostic delete: load ids then remove range by batch (premium safety)
        var ids = await db.Set<OutboxMessage>()
            .Where(m => m.Status == OutboxStatus.Processed && m.ProcessedAtUtc != null && m.ProcessedAtUtc < cutoff)
            .OrderBy(m => m.ProcessedAtUtc)
            .Select(m => m.Id)
            .Take(500)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (ids.Count == 0) return 0;

        var toDelete = ids.Select(id => new OutboxMessage { Id = id }).ToList();
        db.RemoveRange(toDelete);

        try
        {
            await db.SaveChangesAsync(ct).ConfigureAwait(false);
            return ids.Count;
        }
        catch
        {
            // If concurrency hits, ignore; next cleanup cycle will handle.
            return 0;
        }
    }

    private async Task HandleFailureAsync(DbContext db, OutboxMessage msg, Exception ex, CancellationToken ct)
    {
        msg.AttemptCount++;

        var err = $"{ex.GetType().Name}: {ex.Message}";
        msg.LastError = err.Length <= 2000 ? err : err[..2000];

        if (msg.AttemptCount >= _options.MaxAttempts)
        {
            msg.Status = OutboxStatus.DeadLetter;
            msg.LockedUntilUtc = null;
            msg.LockedBy = null;

            await db.SaveChangesAsync(ct).ConfigureAwait(false);

            _logger.LogError(ex,
                "Outbox message moved to DeadLetter (id={Id}, attempts={Attempts}, instance={Instance}).",
                msg.Id, msg.AttemptCount, _options.InstanceId);

            return;
        }

        var delay = OutboxBackoff.ComputeDelay(msg.AttemptCount, _options.BackoffBaseDelay, _options.BackoffMaxDelay);

        msg.Status = OutboxStatus.Pending;
        msg.NextAttemptUtc = DateTime.UtcNow.Add(delay);

        // release lock to allow other instances after NextAttempt
        msg.LockedUntilUtc = null;
        msg.LockedBy = null;

        await db.SaveChangesAsync(ct).ConfigureAwait(false);

        _logger.LogWarning(ex,
            "Outbox dispatch failed (id={Id}, attempt={Attempt}). NextAttempt in {Delay}.",
            msg.Id, msg.AttemptCount, delay);
    }

    private static async Task MarkDeadLetterAsync(DbContext db, OutboxMessage msg, string reason, CancellationToken ct)
    {
        msg.AttemptCount++;
        msg.Status = OutboxStatus.DeadLetter;
        msg.LastError = reason.Length <= 2000 ? reason : reason[..2000];
        msg.LockedUntilUtc = null;
        msg.LockedBy = null;
        await db.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}
