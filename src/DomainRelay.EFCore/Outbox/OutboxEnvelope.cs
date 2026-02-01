namespace DomainRelay.EFCore.Outbox;

/// <summary>
/// Message envelope given to the transport publisher.
/// </summary>
public sealed record OutboxEnvelope(
    Guid OutboxId,
    Guid EventId,
    string TypeKey,
    int Version,
    DateTime OccurredOnUtc,
    string ContentType,
    string PayloadJson,
    string? HeadersJson,
    string? CorrelationId
);
