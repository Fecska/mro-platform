using Mro.Domain.Aggregates.Inventory.Enums;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Inventory;

/// <summary>Stock reservation raised against a specific work-order task.</summary>
public sealed class MaterialReservation : AuditableEntity
{
    public Guid StockItemId { get; private set; }
    public decimal QuantityReserved { get; private set; }
    public decimal QuantityIssued { get; private set; }
    public Guid WorkOrderId { get; private set; }
    public Guid WorkOrderTaskId { get; private set; }
    public Guid ReservedByUserId { get; private set; }
    public DateTimeOffset ReservedAt { get; private set; }
    public ReservationStatus Status { get; private set; } = ReservationStatus.Pending;

    public decimal QuantityOutstanding => QuantityReserved - QuantityIssued;

    private MaterialReservation() { }

    public static MaterialReservation Create(
        Guid stockItemId,
        decimal qty,
        Guid workOrderId,
        Guid workOrderTaskId,
        Guid reservedByUserId,
        Guid organisationId,
        Guid actorId) => new()
    {
        StockItemId       = stockItemId,
        QuantityReserved  = qty,
        QuantityIssued    = 0,
        WorkOrderId       = workOrderId,
        WorkOrderTaskId   = workOrderTaskId,
        ReservedByUserId  = reservedByUserId,
        ReservedAt        = DateTimeOffset.UtcNow,
        OrganisationId    = organisationId,
        CreatedAt         = DateTimeOffset.UtcNow,
        CreatedBy         = actorId,
    };

    internal void RecordIssue(decimal qty)
    {
        QuantityIssued += qty;
        Status = QuantityIssued >= QuantityReserved
            ? ReservationStatus.FullyIssued
            : ReservationStatus.PartiallyIssued;
    }

    internal DomainResult Cancel(Guid actorId)
    {
        if (Status == ReservationStatus.FullyIssued)
            return DomainResult.Failure("Cannot cancel a fully issued reservation.");
        Status    = ReservationStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        return DomainResult.Ok();
    }
}
