namespace DomainRelay.EFCore.Outbox.Admin;

public sealed class OutboxAdminOptions
{
    /// <summary>
    /// Max number of IDs you can requeue/purge in one call to avoid accidental mass operations.
    /// </summary>
    public int MaxBulkOperationSize { get; set; } = 500;
}
