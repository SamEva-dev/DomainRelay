# DomainRelay EF Core Outbox (Premium)

## Goals
- Persist domain events in the same transaction as your state changes (SaveChanges).
- Dispatch reliably with retries/backoff, leasing, dead-letter.
- Safe deserialization via allowlist registry.

## 1) Add Outbox entity mapping
In your DbContext:

```csharp
using DomainRelay.EFCore.Outbox;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.AddDomainRelayOutbox(tableName: "OutboxMessages");
}
