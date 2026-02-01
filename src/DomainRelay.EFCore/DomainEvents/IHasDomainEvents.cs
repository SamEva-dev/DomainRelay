namespace DomainRelay.EFCore.DomainEvents;

/// <summary>
/// Implemented by aggregate roots/entities that produce domain events.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
