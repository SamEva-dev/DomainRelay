using System.Text.Json;

namespace DomainRelay.EFCore.Outbox.Hosting;

public sealed class DomainRelayOutboxBuilder<TDbContext>
    where TDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    internal Action<OutboxOptions>? ConfigureOutboxOptions { get; private set; }
    internal Action<OutboxTypeRegistry>? ConfigureRegistry { get; private set; }
    internal JsonSerializerOptions? JsonOptions { get; private set; }

    internal Action<IServiceProvider, Microsoft.EntityFrameworkCore.DbContextOptionsBuilder>? ConfigureDbContext { get; private set; }

    internal DomainRelayOutboxBuilder() { }

    public DomainRelayOutboxBuilder<TDbContext> WithOutboxOptions(Action<OutboxOptions> configure)
    {
        ConfigureOutboxOptions = configure;
        return this;
    }

    public DomainRelayOutboxBuilder<TDbContext> WithTypeRegistry(Action<OutboxTypeRegistry> configure)
    {
        ConfigureRegistry = configure;
        return this;
    }

    public DomainRelayOutboxBuilder<TDbContext> WithJsonOptions(JsonSerializerOptions options)
    {
        JsonOptions = options;
        return this;
    }

    /// <summary>
    /// REQUIRED. Configures both DbContext and DbContextFactory with the same options (provider/connection string).
    /// </summary>
    public DomainRelayOutboxBuilder<TDbContext> WithDbContextOptions(Action<IServiceProvider, Microsoft.EntityFrameworkCore.DbContextOptionsBuilder> configure)
    {
        ConfigureDbContext = configure;
        return this;
    }
}
