namespace DomainRelay.EFCore.DomainEvents;

/// <summary>
/// Represents an entity or aggregate root that can hold domain events.
/// </summary>
/// <remarks>
/// EF Core interceptors can inspect tracked entities implementing this interface,
/// read their domain events and clear them after dispatching or outbox persistence.
/// </remarks>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the domain events currently raised by the entity.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all currently stored domain events.
    /// </summary>
    void ClearDomainEvents();
}