using DomainRelay.EFCore.DomainEvents;
using DomainRelay.EFCore.Outbox.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DomainRelay.EFCore.Outbox;

/// <summary>
/// Captures domain events from tracked entities and persists them to OutboxMessages
/// in the same SaveChanges transaction.
/// </summary>
public sealed class OutboxSaveChangesInterceptor : SaveChangesInterceptor
{
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
            EnqueueOutbox(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            EnqueueOutbox(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void EnqueueOutbox(DbContext db)
    {
        // Collect events from aggregates
        var entities = db.ChangeTracker
            .Entries()
            .Select(e => e.Entity)
            .OfType<IHasDomainEvents>()
            .ToArray();

        if (entities.Length == 0) return;

        var events = new List<IDomainEvent>(64);

        foreach (var entity in entities)
        {
            if (entity.DomainEvents.Count > 0)
            {
                events.AddRange(entity.DomainEvents);
                entity.ClearDomainEvents();
            }
        }

        if (events.Count == 0) return;

        var now = DateTime.UtcNow;

        foreach (var ev in events)
        {
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
    }
}
