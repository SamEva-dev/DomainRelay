namespace DomainRelay.EFCore.Outbox;

public sealed class OutboxOptions
{
    /// <summary>DB schema name. Null means default schema.</summary>
    public string? Schema { get; set; }

    /// <summary>Outbox table name.</summary>
    public string TableName { get; set; } = "OutboxMessages";

    /// <summary>How often the dispatcher polls the DB.</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>How often cleanup runs.</summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>How many messages to claim per batch.</summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>How long a claim lease is valid.</summary>
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Max attempts before dead-letter.</summary>
    public int MaxAttempts { get; set; } = 12;

    /// <summary>Base delay for backoff.</summary>
    public TimeSpan BackoffBaseDelay { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>Max delay for backoff.</summary>
    public TimeSpan BackoffMaxDelay { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Cleanup: delete Processed messages older than this.
    /// Set to null to disable deletion.
    /// </summary>
    public TimeSpan? ProcessedRetention { get; set; } = TimeSpan.FromDays(7);

    /// <summary>Worker instance id for lease ownership.</summary>
    public string InstanceId { get; set; } = $"{Environment.MachineName}-{Guid.NewGuid():N}".Substring(0, 32);

    /// <summary>
    /// When true, dispatcher will log debug details more often (may be noisy).
    /// </summary>
    public bool VerboseLogging { get; set; } = false;
}
