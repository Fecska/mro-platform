using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Inventory;

/// <summary>Record of parts physically issued from stock to a work-order task.</summary>
public sealed class MaterialIssue : AuditableEntity
{
    public Guid StockItemId { get; private set; }
    public Guid? ReservationId { get; private set; }
    public decimal QuantityIssued { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? SerialNumber { get; private set; }
    public Guid IssuedByUserId { get; private set; }
    public DateTimeOffset IssuedAt { get; private set; }
    public Guid WorkOrderId { get; private set; }
    public Guid WorkOrderTaskId { get; private set; }
    public string? IssueSlipRef { get; private set; }

    private MaterialIssue() { }

    public static MaterialIssue Create(
        Guid stockItemId,
        decimal qty,
        Guid workOrderId,
        Guid workOrderTaskId,
        Guid issuedByUserId,
        Guid organisationId,
        Guid actorId,
        Guid? reservationId = null,
        string? batchNumber = null,
        string? serialNumber = null,
        string? issueSlipRef = null) => new()
    {
        StockItemId     = stockItemId,
        ReservationId   = reservationId,
        QuantityIssued  = qty,
        WorkOrderId     = workOrderId,
        WorkOrderTaskId = workOrderTaskId,
        IssuedByUserId  = issuedByUserId,
        IssuedAt        = DateTimeOffset.UtcNow,
        OrganisationId  = organisationId,
        BatchNumber     = batchNumber,
        SerialNumber    = serialNumber,
        IssueSlipRef    = issueSlipRef,
        CreatedAt       = DateTimeOffset.UtcNow,
        CreatedBy       = actorId,
    };
}
