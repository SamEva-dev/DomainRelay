using DomainRelay.EFCore.Outbox;

namespace DomainRelay.Transport.RabbitMQ.Routing;

/// <summary>
/// Determines where/how to publish a given outbox envelope.
/// </summary>
public interface IOutboxRouter
{
    OutboxRoute Route(OutboxEnvelope envelope);
}
