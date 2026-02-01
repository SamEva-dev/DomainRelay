namespace DomainRelay.Transport.RabbitMQ.Routing;

public sealed record OutboxRoute(
    string Exchange,
    string RoutingKey,
    bool Mandatory = false,
    bool Persistent = true
);
