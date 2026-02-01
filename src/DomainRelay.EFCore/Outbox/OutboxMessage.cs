using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DomainRelay.EFCore.Outbox;

/// <summary>
/// Durable outbox record (provider-agnostic design).
/// </summary>
public sealed class OutboxMessage
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>Logical event id (for downstream idempotency).</summary>
    public Guid EventId { get; set; }

    /// <summary>Event type key (resolved through registry).</summary>
    [MaxLength(256)]
    public string TypeKey { get; set; } = string.Empty;

    /// <summary>Version for payload schema evolution.</summary>
    public int Version { get; set; } = 1;

    /// <summary>Occurred time (UTC).</summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>When it was inserted into the outbox (UTC).</summary>
    public DateTime EnqueuedAtUtc { get; set; }

    /// <summary>When successfully dispatched (UTC).</summary>
    public DateTime? ProcessedAtUtc { get; set; }

    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;

    /// <summary>How many times dispatch has been attempted.</summary>
    public int AttemptCount { get; set; }

    /// <summary>Next eligible time for dispatch (UTC), supports backoff.</summary>
    public DateTime NextAttemptUtc { get; set; }

    /// <summary>Lease / lock expiry (UTC). If expired, other workers may claim.</summary>
    public DateTime? LockedUntilUtc { get; set; }

    /// <summary>Identifier of the worker that currently owns the lease.</summary>
    [MaxLength(128)]
    public string? LockedBy { get; set; }

    /// <summary>Last error summary (truncated).</summary>
    [MaxLength(2000)]
    public string? LastError { get; set; }

    /// <summary>Arbitrary headers for transport (json).</summary>
    public string? HeadersJson { get; set; }

    /// <summary>Payload json.</summary>
    [Column(TypeName = "text")]
    public string PayloadJson { get; set; } = string.Empty;

    /// <summary>Content type (default: application/json).</summary>
    [MaxLength(128)]
    public string ContentType { get; set; } = "application/json";

    /// <summary>Optional correlation id for tracing.</summary>
    [MaxLength(64)]
    public string? CorrelationId { get; set; }

    /// <summary>Optimistic concurrency token.</summary>
    [Timestamp]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
