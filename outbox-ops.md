# Outbox Operations (Premium)

## Health/Stats
Use `IOutboxAdmin.GetStatsAsync()` to show:
- Pending / Processing / Processed / DeadLetter

## Dead-letter inspection
`GetDeadLettersAsync(take: 50)` returns the most recent dead letters.
You can display:
- Id, TypeKey, AttemptCount, LastError, EnqueuedAtUtc, OccurredOnUtc

## Requeue
`RequeueAsync(ids, resetAttempts: false)`:
- Status -> Pending
- NextAttemptUtc -> now
- clears lock + error

Optionally `resetAttempts: true` for operator override.

## Purge processed
`PurgeProcessedOlderThanAsync(cutoffUtc)` deletes in batches to be provider-safe.

## Hard delete
`DeleteByIdsAsync` is intentionally loud; use for GDPR/PII removal.

## Operator safety
OutboxAdminOptions.MaxBulkOperationSize prevents accidental mass actions.


IMPORTANT : Où mettre la table (schema + naming)

Dans ton DbContext :

using DomainRelay.EFCore.Outbox;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Schema + Table from options (best practice)
    modelBuilder.AddDomainRelayOutbox(tableName: "outbox_messages", schema: "ops");
}


Tu peux harmoniser : ops.outbox_messages comme dans tes schemas (ops/iam/etc.).

8) Exemple complet d’intégration (recommandé)
using DomainRelay.EFCore;
using DomainRelay.EFCore.Outbox;

// Transport
services.AddSingleton<DomainRelay.EFCore.Outbox.Abstractions.IOutboxPublisher, MyBusPublisher>();

// Outbox premium + DbContext config obligatoire
services.AddDomainRelayEfCoreOutbox<MyDbContext>(b =>
{
    b.WithDbContextOptions((sp, o) =>
    {
        o.UseNpgsql(builder.Configuration.GetConnectionString("db"));
        // Optionnel: o.EnableSensitiveDataLogging(false);
    });

    b.WithOutboxOptions(o =>
    {
        o.Schema = "ops";
        o.TableName = "outbox_messages";
        o.InstanceId = $"locaguest-api-{Environment.MachineName}";
        o.BatchSize = 200;
        o.PollingInterval = TimeSpan.FromSeconds(1);
        o.LeaseDuration = TimeSpan.FromSeconds(30);
        o.MaxAttempts = 12;
        o.ProcessedRetention = TimeSpan.FromDays(14);
        o.VerboseLogging = false;
    });

    b.WithTypeRegistry(reg =>
    {
        reg.Register<UserCreated>("iam.user.created.v1");
        reg.Register<UserDeleted>("iam.user.deleted.v1");
    });
});