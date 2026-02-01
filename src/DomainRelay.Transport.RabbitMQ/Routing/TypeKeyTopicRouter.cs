using DomainRelay.EFCore.Outbox;

namespace DomainRelay.Transport.RabbitMQ.Routing;

/// <summary>
/// Default router:
/// - Exchange = options.ExchangeName
/// - RoutingKey = envelope.TypeKey (normalized to topic-friendly)
/// </summary>
public sealed class TypeKeyTopicRouter : IOutboxRouter
{
    private readonly RabbitMqPublisherOptions _options;

    public TypeKeyTopicRouter(RabbitMqPublisherOptions options) => _options = options;

    public OutboxRoute Route(OutboxEnvelope envelope)
    {
        var rk = NormalizeToTopic(envelope.TypeKey);
        return new OutboxRoute(_options.ExchangeName, rk, _options.Mandatory, Persistent: true);
    }

    private static string NormalizeToTopic(string typeKey)
    {
        // Topic keys are often dot-separated. We keep dots and replace unsafe chars with '-'.
        var chars = typeKey.Select(ch =>
        {
            if (char.IsLetterOrDigit(ch)) return ch;
            if (ch is '.' or '-' or '_' ) return ch;
            return '-';
        }).ToArray();

        var s = new string(chars);

        // avoid empty routing key
        return string.IsNullOrWhiteSpace(s) ? "domainrelay.unknown" : s;
    }
}
