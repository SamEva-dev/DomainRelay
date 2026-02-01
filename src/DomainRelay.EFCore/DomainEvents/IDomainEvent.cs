namespace DomainRelay.EFCore.DomainEvents;

/// <summary>
/// Marker interface for domain events to be persisted to the Outbox.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Unique event id for idempotency.</summary>
    Guid EventId { get; }

    /// <summary>When the event occurred (UTC).</summary>
    DateTime OccurredOnUtc { get; }
}
