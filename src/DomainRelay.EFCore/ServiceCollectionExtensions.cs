using DomainRelay.Abstractions;
using DomainRelay.EFCore.Abstractions;
using DomainRelay.EFCore.Behaviors;
using DomainRelay.EFCore.Outbox;
using DomainRelay.EFCore.Outbox.Abstractions;
using DomainRelay.EFCore.Outbox.Admin;
using DomainRelay.EFCore.Outbox.Dispatching;
using DomainRelay.EFCore.Outbox.Hosting;
using DomainRelay.EFCore.Outbox.Serialization;
using DomainRelay.EFCore.Resolvers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DomainRelay.EFCore;

public static class ServiceCollectionExtensions
{
    // --------------------------
    // Transactions
    // --------------------------

    public static IServiceCollection AddDomainRelayEfCoreTransaction(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        return services;
    }

    public static IServiceCollection AddDomainRelayEfCoreTransactionResolver<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        services.AddScoped<IDomainRelayDbContextResolver, SingleDbContextResolver<TDbContext>>();
        services.AddDomainRelayEfCoreTransaction();
        return services;
    }

    public static IServiceCollection AddDomainRelayEfCoreTransactionResolver(
        this IServiceCollection services,
        Action<DictionaryDbContextResolver> configure)
    {
        services.AddScoped<IDomainRelayDbContextResolver>(sp =>
        {
            var r = new DictionaryDbContextResolver(sp);
            configure(r);
            return r;
        });

        services.AddDomainRelayEfCoreTransaction();
        return services;
    }

    // --------------------------
    // Outbox (Premium, Zero foot-gun)
    // --------------------------

    public static IServiceCollection AddDomainRelayEfCoreOutbox<TDbContext>(
        this IServiceCollection services,
        Action<DomainRelayOutboxBuilder<TDbContext>> build)
        where TDbContext : DbContext
    {
        var builder = new DomainRelayOutboxBuilder<TDbContext>();
        build(builder);

        if (builder.ConfigureDbContext is null)
            throw new InvalidOperationException("WithDbContextOptions(...) is required for DomainRelay Outbox.");

        // Outbox options
        var outboxOptions = new OutboxOptions();
        builder.ConfigureOutboxOptions?.Invoke(outboxOptions);
        services.AddSingleton(outboxOptions);

        // Admin options
        services.AddSingleton(new OutboxAdminOptions());

        // Type registry allowlist
        services.AddSingleton<IOutboxTypeRegistry>(_ =>
        {
            var reg = new OutboxTypeRegistry();
            builder.ConfigureRegistry?.Invoke(reg);
            return reg;
        });

        // Serializer
        services.AddSingleton<IOutboxSerializer>(_ => new SystemTextJsonOutboxSerializer(builder.JsonOptions));

        // Interceptor
        services.AddSingleton<OutboxSaveChangesInterceptor>();

        // Configure DbContext + Factory with same options + interceptor
        services.AddDbContext<TDbContext>((sp, o) =>
        {
            builder.ConfigureDbContext!(sp, o);
            o.AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>());
        });

        services.AddDbContextFactory<TDbContext>((sp, o) =>
        {
            builder.ConfigureDbContext!(sp, o);
            o.AddInterceptors(sp.GetRequiredService<OutboxSaveChangesInterceptor>());
        });

        // Dispatcher + hosted service
        services.AddSingleton<OutboxDispatcher<TDbContext>>();
        services.AddHostedService<OutboxDispatcherHostedService<TDbContext>>();

        // Admin API
        services.AddSingleton<IOutboxAdmin, OutboxAdminService<TDbContext>>();

        return services;
    }
}
