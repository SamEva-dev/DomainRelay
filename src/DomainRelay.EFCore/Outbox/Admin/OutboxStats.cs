namespace DomainRelay.EFCore.Outbox.Admin;

public sealed record OutboxStats(
    long Pending,
    long Processing,
    long Processed,
    long DeadLetter,
    long Total);
