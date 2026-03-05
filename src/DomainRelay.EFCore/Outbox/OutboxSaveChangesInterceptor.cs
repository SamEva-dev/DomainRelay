using DomainRelay.EFCore.DomainEvents;
using DomainRelay.EFCore.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Runtime.CompilerServices;

namespace DomainRelay.EFCore.Outbox;

/// <summary>
/// Captures domain events from tracked entities and persists them to OutboxMessages
/// in the same SaveChanges transaction.
/// </summary>
public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly ConditionalWeakTable<DbContext, PendingClearState> _pendingClearByContext = new();

    private readonly IOutboxTypeRegistry _typeRegistry;
    private readonly IOutboxSerializer _serializer;

    public OutboxSaveChangesInterceptor(IOutboxTypeRegistry typeRegistry, IOutboxSerializer serializer)
    {
        _typeRegistry = typeRegistry;
        _serializer = serializer;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            var entitiesToClear = EnqueueOutbox(eventData.Context);
            if (entitiesToClear.Count > 0)
                _pendingClearByContext.AddOrUpdate(eventData.Context, new PendingClearState(entitiesToClear));
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            var entitiesToClear = EnqueueOutbox(eventData.Context);
            if (entitiesToClear.Count > 0)
                _pendingClearByContext.AddOrUpdate(eventData.Context, new PendingClearState(entitiesToClear));
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is not null && _pendingClearByContext.TryGetValue(eventData.Context, out var pending))
        {
            foreach (var entity in pending.Entities)
                entity.ClearDomainEvents();

            _pendingClearByContext.Remove(eventData.Context);
        }

        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null && _pendingClearByContext.TryGetValue(eventData.Context, out var pending))
        {
            foreach (var entity in pending.Entities)
                entity.ClearDomainEvents();

            _pendingClearByContext.Remove(eventData.Context);
        }

        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is not null)
            _pendingClearByContext.Remove(eventData.Context);

        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            _pendingClearByContext.Remove(eventData.Context);

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private List<IHasDomainEvents> EnqueueOutbox(DbContext db)
    {
        // Collect events from aggregates
        var entities = db.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .ToArray();

        if (entities.Length == 0) return new List<IHasDomainEvents>(0);

        var alreadyTrackedEventIds = db.ChangeTracker
            .Entries<OutboxMessage>()
            .Select(e => e.Entity.EventId)
            .ToHashSet();

        var events = new List<IDomainEvent>(64);
        var entitiesToClear = new List<IHasDomainEvents>(entities.Length);

        foreach (var entity in entities)
        {
            if (entity.DomainEvents.Count > 0)
            {
                events.AddRange(entity.DomainEvents);
                entitiesToClear.Add(entity);
            }
        }

        if (events.Count == 0) return new List<IHasDomainEvents>(0);

        var now = DateTime.UtcNow;

        foreach (var ev in events)
        {
            if (!alreadyTrackedEventIds.Add(ev.EventId))
                continue;

            var evType = ev.GetType();
            var typeKey = _typeRegistry.GetTypeKey(evType);
            var payload = _serializer.Serialize(ev, evType);

            db.Set<OutboxMessage>().Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                EventId = ev.EventId,
                TypeKey = typeKey,
                Version = 1,
                OccurredOnUtc = ev.OccurredOnUtc,
                EnqueuedAtUtc = now,
                NextAttemptUtc = now,
                Status = OutboxStatus.Pending,
                PayloadJson = payload,
                ContentType = "application/json"
            });
        }

        return entitiesToClear;
    }

    private sealed record PendingClearState(IReadOnlyList<IHasDomainEvents> Entities);
}
