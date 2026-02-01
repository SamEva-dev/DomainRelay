namespace DomainRelay.Transport.RabbitMQ;

public sealed class RabbitMqPublisherOptions
{
    // Connection
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    // Exchange
    public string ExchangeName { get; set; } = "domainrelay.events";
    public string ExchangeType { get; set; } = "topic";
    public bool DeclareExchange { get; set; } = true;
    public bool ExchangeDurable { get; set; } = true;

    // Publishing behavior
    public bool PublisherConfirms { get; set; } = true;
    public TimeSpan ConfirmsTimeout { get; set; } = TimeSpan.FromSeconds(5);

    public bool Mandatory { get; set; } = false;

    // Auto recovery (if supported by client runtime)
    public bool AutomaticRecoveryEnabled { get; set; } = true;

    // Headers
    public bool InjectW3CTracingHeaders { get; set; } = true;
}
