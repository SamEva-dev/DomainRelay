using DomainRelay.EFCore.Outbox.Abstractions;
using DomainRelay.Transport.RabbitMQ.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DomainRelay.Transport.RabbitMQ;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomainRelayRabbitMqPublisher(
        this IServiceCollection services,
        Action<RabbitMqPublisherOptions> configure,
        Action<IServiceProvider, RabbitMqPublisherOptions>? postConfigure = null)
    {
        var options = new RabbitMqPublisherOptions();
        configure(options);
        postConfigure?.Invoke(services.BuildServiceProvider(), options); // optional advanced hook
        services.AddSingleton(options);

        // Router: default topic router
        services.TryAddSingleton<IOutboxRouter, TypeKeyTopicRouter>();

        // Publisher
        services.AddSingleton<RabbitMqOutboxPublisher>();
        services.AddSingleton<IOutboxPublisher>(sp => sp.GetRequiredService<RabbitMqOutboxPublisher>());

        // Logging is app responsibility; we just require it
        services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }
}
