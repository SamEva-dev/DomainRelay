using Microsoft.EntityFrameworkCore;

namespace DomainRelay.EFCore.Outbox;

public static class ModelBuilderExtensions
{
    public static ModelBuilder AddDomainRelayOutbox(
        this ModelBuilder modelBuilder,
        string tableName = "OutboxMessages",
        string? schema = null)
    {
        var e = modelBuilder.Entity<OutboxMessage>();

        if (string.IsNullOrWhiteSpace(schema))
            e.ToTable(tableName);
        else
            e.ToTable(tableName, schema);

        e.HasKey(x => x.Id);

        e.HasIndex(x => new { x.Status, x.NextAttemptUtc });
        e.HasIndex(x => x.EventId);
        e.HasIndex(x => new { x.LockedUntilUtc, x.LockedBy });

        e.Property(x => x.TypeKey).HasMaxLength(256).IsRequired();
        e.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
        e.Property(x => x.LockedBy).HasMaxLength(128);
        e.Property(x => x.LastError).HasMaxLength(2000);
        e.Property(x => x.CorrelationId).HasMaxLength(64);

        e.Property(x => x.PayloadJson).IsRequired();

        e.Property(x => x.RowVersion).IsRowVersion();

        return modelBuilder;
    }
}
