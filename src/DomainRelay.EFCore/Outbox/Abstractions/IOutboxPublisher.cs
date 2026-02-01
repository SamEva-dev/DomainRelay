namespace DomainRelay.EFCore.Outbox.Abstractions;

/// <summary>
/// Transport abstraction. You implement this to publish events to your bus
/// (RabbitMQ, Kafka, HTTP webhook, SignalR, etc.)
/// </summary>
public interface IOutboxPublisher
{
    Task PublishAsync(OutboxEnvelope envelope, CancellationToken ct);
}
