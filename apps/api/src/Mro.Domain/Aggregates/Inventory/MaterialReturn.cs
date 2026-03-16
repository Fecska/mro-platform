using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Inventory;

/// <summary>Record of parts physically returned from a work-order task back to stock.</summary>
public sealed class MaterialReturn : AuditableEntity
{
    public Guid StockItemId { get; private set; }

    /// <summary>Optional reference to the original issue being reversed.</summary>
    public Guid? OriginalIssueId { get; private set; }

    public decimal QuantityReturned { get; private set; }

    public string? BatchNumber { get; private set; }

    public string? SerialNumber { get; private set; }

    public Guid ReturnedByUserId { get; private set; }

    public DateTimeOffset ReturnedAt { get; private set; }

    public Guid WorkOrderId { get; private set; }

    public Guid WorkOrderTaskId { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    private MaterialReturn() { }

    internal static MaterialReturn Create(
        Guid stockItemId,
        decimal qty,
        Guid workOrderId,
        Guid workOrderTaskId,
        Guid returnedByUserId,
        string reason,
        Guid organisationId,
        Guid actorId,
        Guid? originalIssueId = null,
        string? batchNumber   = null,
        string? serialNumber  = null) => new()
    {
        StockItemId       = stockItemId,
        OriginalIssueId   = originalIssueId,
        QuantityReturned  = qty,
        WorkOrderId       = workOrderId,
        WorkOrderTaskId   = workOrderTaskId,
        ReturnedByUserId  = returnedByUserId,
        ReturnedAt        = DateTimeOffset.UtcNow,
        Reason            = reason.Trim(),
        OrganisationId    = organisationId,
        BatchNumber       = batchNumber,
        SerialNumber      = serialNumber,
        CreatedAt         = DateTimeOffset.UtcNow,
        CreatedBy         = actorId,
    };
}
