using System.Text;
using DomainRelay.EFCore.Outbox;
using DomainRelay.EFCore.Outbox.Abstractions;
using DomainRelay.Transport.RabbitMQ.Internal;
using DomainRelay.Transport.RabbitMQ.Routing;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DomainRelay.Transport.RabbitMQ;

public sealed class RabbitMqOutboxPublisher : IOutboxPublisher, IDisposable
{
    private readonly RabbitMqPublisherOptions _options;
    private readonly IOutboxRouter _router;
    private readonly ILogger<RabbitMqOutboxPublisher> _logger;

    private readonly object _sync = new();
    private IConnection? _connection;

    public RabbitMqOutboxPublisher(
        RabbitMqPublisherOptions options,
        IOutboxRouter router,
        ILogger<RabbitMqOutboxPublisher> logger)
    {
        _options = options;
        _router = router;
        _logger = logger;
    }

    public Task PublishAsync(OutboxEnvelope envelope, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        EnsureConnection();

        var route = _router.Route(envelope);

        // Channel/model is NOT thread-safe => create per publish
        using var channel = _connection!.CreateModel();

        if (_options.DeclareExchange)
        {
            channel.ExchangeDeclare(
                exchange: route.Exchange,
                type: _options.ExchangeType,
                durable: _options.ExchangeDurable,
                autoDelete: false,
                arguments: null);
        }

        if (_options.PublisherConfirms)
        {
            channel.ConfirmSelect();
        }

        var props = channel.CreateBasicProperties();
        props.ContentType = envelope.ContentType;
        props.DeliveryMode = route.Persistent ? (byte)2 : (byte)1; // 2=persistent
        props.MessageId = envelope.EventId.ToString("D");
        props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        props.Headers = HeadersHelper.BuildHeaders(envelope.HeadersJson, _options.InjectW3CTracingHeaders);

        if (!string.IsNullOrWhiteSpace(envelope.CorrelationId))
            props.CorrelationId = envelope.CorrelationId;

        var body = Encoding.UTF8.GetBytes(envelope.PayloadJson);

        channel.BasicPublish(
            exchange: route.Exchange,
            routingKey: route.RoutingKey,
            mandatory: route.Mandatory,
            basicProperties: props,
            body: body);

        if (_options.PublisherConfirms)
        {
            // Will throw if broker didn't confirm in time or nacked
            channel.WaitForConfirmsOrDie(_options.ConfirmsTimeout);
        }

        _logger.LogDebug(
            "RabbitMQ published outboxId={OutboxId} eventId={EventId} typeKey={TypeKey} exchange={Exchange} rk={RoutingKey}",
            envelope.OutboxId, envelope.EventId, envelope.TypeKey, route.Exchange, route.RoutingKey);

        return Task.CompletedTask;
    }

    private void EnsureConnection()
    {
        if (_connection is { IsOpen: true })
            return;

        lock (_sync)
        {
            if (_connection is { IsOpen: true })
                return;

            _connection?.Dispose();

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                VirtualHost = _options.VirtualHost,
                UserName = _options.UserName,
                Password = _options.Password,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = _options.AutomaticRecoveryEnabled
            };

            _connection = factory.CreateConnection("DomainRelay.OutboxPublisher");

            _logger.LogInformation(
                "RabbitMQ connection established to {Host}:{Port} vhost={VHost} exchange={Exchange}",
                _options.HostName, _options.Port, _options.VirtualHost, _options.ExchangeName);
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}
