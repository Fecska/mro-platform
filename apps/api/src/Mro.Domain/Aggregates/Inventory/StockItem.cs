using Mro.Domain.Aggregates.Inventory.Enums;
using Mro.Domain.Aggregates.Inventory.Events;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Inventory;

/// <summary>
/// On-hand stock of a specific Part at a specific BinLocation.
/// Controls reservations and issues; enforces non-negative stock invariant.
/// </summary>
public sealed class StockItem : AuditableEntity
{
    private readonly List<MaterialReservation> _reservations = [];
    private readonly List<MaterialIssue>       _issues       = [];
    private readonly List<MaterialReturn>      _returns      = [];

    public Guid PartId { get; private set; }
    public Guid BinLocationId { get; private set; }
    public decimal QuantityOnHand { get; private set; }
    public decimal UnitCost { get; private set; }
    public string? BatchNumber { get; private set; }
    public string? SerialNumber { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }

    public IReadOnlyList<MaterialReservation> Reservations => _reservations.AsReadOnly();
    public IReadOnlyList<MaterialIssue>       Issues       => _issues.AsReadOnly();
    public IReadOnlyList<MaterialReturn>      Returns      => _returns.AsReadOnly();

    public decimal QuantityReserved  =>
        _reservations
            .Where(r => r.Status is ReservationStatus.Pending or ReservationStatus.PartiallyIssued)
            .Sum(r => r.QuantityOutstanding);

    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;

    private StockItem() { }

    public static StockItem Create(
        Guid partId,
        Guid binLocationId,
        decimal qtyOnHand,
        decimal unitCost,
        Guid organisationId,
        Guid actorId,
        string? batchNumber = null,
        string? serialNumber = null,
        DateTimeOffset? expiresAt = null) => new()
    {
        PartId         = partId,
        BinLocationId  = binLocationId,
        QuantityOnHand = qtyOnHand,
        UnitCost       = unitCost,
        OrganisationId = organisationId,
        BatchNumber    = batchNumber,
        SerialNumber   = serialNumber,
        ExpiresAt      = expiresAt,
        CreatedAt      = DateTimeOffset.UtcNow,
        CreatedBy      = actorId,
    };

    // ── Receive ────────────────────────────────────────────────────────────

    public DomainResult Receive(decimal qty, decimal unitCost, Guid actorId)
    {
        if (qty <= 0) return DomainResult.Failure("Receive quantity must be positive.");
        QuantityOnHand += qty;
        UnitCost        = unitCost;
        UpdatedAt       = DateTimeOffset.UtcNow;
        UpdatedBy       = actorId;
        return DomainResult.Ok();
    }

    // ── Reserve ────────────────────────────────────────────────────────────

    public DomainResult Reserve(
        decimal qty,
        Guid workOrderId,
        Guid workOrderTaskId,
        Guid reservedByUserId,
        Guid actorId)
    {
        if (qty <= 0)
            return DomainResult.Failure("Reserve quantity must be positive.");
        if (QuantityAvailable < qty)
            return DomainResult.Failure(
                $"Insufficient available stock. Available: {QuantityAvailable}, Requested: {qty}.");

        var reservation = MaterialReservation.Create(
            Id, qty, workOrderId, workOrderTaskId, reservedByUserId, OrganisationId, actorId);
        _reservations.Add(reservation);

        RaiseDomainEvent(new StockReservedEvent
        {
            ActorId          = actorId,
            OrganisationId   = OrganisationId,
            EntityType       = "StockItem",
            EntityId         = Id,
            EventType        = "STOCK_RESERVED",
            Description      = $"Stock reserved: {qty} units for work order task {workOrderTaskId}.",
            StockItemId      = Id,
            QuantityReserved = qty,
            WorkOrderId      = workOrderId,
            WorkOrderTaskId  = workOrderTaskId,
        });

        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        return DomainResult.Ok();
    }

    // ── Issue ──────────────────────────────────────────────────────────────

    public DomainResult Issue(
        Guid? reservationId,
        decimal qty,
        Guid workOrderId,
        Guid workOrderTaskId,
        Guid issuedByUserId,
        Guid actorId,
        string? batchNumber    = null,
        string? serialNumber   = null,
        string? issueSlipRef   = null)
    {
        if (qty <= 0)
            return DomainResult.Failure("Issue quantity must be positive.");
        if (QuantityOnHand < qty)
            return DomainResult.Failure(
                $"Insufficient on-hand stock. On hand: {QuantityOnHand}, Requested: {qty}.");

        MaterialReservation? reservation = null;
        if (reservationId.HasValue)
        {
            reservation = _reservations.FirstOrDefault(r => r.Id == reservationId.Value);
            if (reservation is null)
                return DomainResult.Failure("Reservation not found on this stock item.");
            if (reservation.Status == ReservationStatus.Cancelled)
                return DomainResult.Failure("Cannot issue against a cancelled reservation.");
            if (reservation.Status == ReservationStatus.FullyIssued)
                return DomainResult.Failure("Reservation is already fully issued.");
        }

        var issue = MaterialIssue.Create(
            Id, qty, workOrderId, workOrderTaskId, issuedByUserId, OrganisationId, actorId,
            reservationId, batchNumber, serialNumber, issueSlipRef);
        _issues.Add(issue);

        QuantityOnHand -= qty;
        reservation?.RecordIssue(qty);

        RaiseDomainEvent(new MaterialIssuedEvent
        {
            ActorId         = actorId,
            OrganisationId  = OrganisationId,
            EntityType      = "StockItem",
            EntityId        = Id,
            EventType       = "MATERIAL_ISSUED",
            Description     = $"Issued {qty} units to work order task {workOrderTaskId}.",
            StockItemId     = Id,
            QuantityIssued  = qty,
            WorkOrderId     = workOrderId,
            WorkOrderTaskId = workOrderTaskId,
        });

        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        return DomainResult.Ok();
    }

    // ── Adjust ─────────────────────────────────────────────────────────────

    public DomainResult Adjust(decimal delta, string reason, Guid actorId)
    {
        if (QuantityOnHand + delta < 0)
            return DomainResult.Failure("Adjustment would result in negative stock.");
        QuantityOnHand += delta;
        UpdatedAt       = DateTimeOffset.UtcNow;
        UpdatedBy       = actorId;
        return DomainResult.Ok();
    }

    // ── Cancel reservation ─────────────────────────────────────────────────

    public DomainResult CancelReservation(Guid reservationId, Guid actorId)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == reservationId);
        if (reservation is null)
            return DomainResult.Failure("Reservation not found.");
        return reservation.Cancel(actorId);
    }

    // ── Return ─────────────────────────────────────────────────────────────

    public DomainResult Return(
        decimal qty,
        Guid workOrderId,
        Guid workOrderTaskId,
        Guid returnedByUserId,
        string reason,
        Guid actorId,
        Guid? originalIssueId = null,
        string? batchNumber   = null,
        string? serialNumber  = null)
    {
        if (qty <= 0)
            return DomainResult.Failure("Return quantity must be positive.");

        if (originalIssueId.HasValue)
        {
            var original = _issues.FirstOrDefault(i => i.Id == originalIssueId.Value);
            if (original is null)
                return DomainResult.Failure("Original issue not found on this stock item.");
            if (qty > original.QuantityIssued)
                return DomainResult.Failure(
                    $"Return quantity {qty} exceeds original issued quantity {original.QuantityIssued}.");
        }

        var ret = MaterialReturn.Create(
            Id, qty, workOrderId, workOrderTaskId, returnedByUserId,
            reason, OrganisationId, actorId,
            originalIssueId, batchNumber, serialNumber);
        _returns.Add(ret);

        QuantityOnHand += qty;
        UpdatedAt       = DateTimeOffset.UtcNow;
        UpdatedBy       = actorId;
        return DomainResult.Ok();
    }
}
