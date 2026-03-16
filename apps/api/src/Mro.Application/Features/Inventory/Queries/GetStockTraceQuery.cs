using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Inventory.Queries;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed record StockTraceEventDto(
    string EventType,          // Receipt | Reservation | Issue | Return | Adjustment
    DateTimeOffset OccurredAt,
    decimal Quantity,
    decimal? QuantityOnHandAfter,
    Guid? WorkOrderId,
    Guid? WorkOrderTaskId,
    string? Reference,         // IssueSlipRef / ReservationId / Reason
    string? BatchNumber,
    string? SerialNumber,
    Guid ActorId);

public sealed record StockTraceDto(
    Guid StockItemId,
    Guid PartId,
    string? BatchNumber,
    string? SerialNumber,
    DateTimeOffset? ExpiresAt,
    IReadOnlyList<StockTraceEventDto> Events);

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetStockTraceQuery(Guid StockItemId)
    : IRequest<Result<StockTraceDto>>;

public sealed class GetStockTraceQueryHandler(
    IStockItemRepository stockItems,
    ICurrentUserService currentUser)
    : IRequestHandler<GetStockTraceQuery, Result<StockTraceDto>>
{
    public async Task<Result<StockTraceDto>> Handle(GetStockTraceQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<StockTraceDto>(Error.Forbidden("Organisation context is required."));

        var item = await stockItems.GetByIdAsync(request.StockItemId, currentUser.OrganisationId.Value, ct);
        if (item is null)
            return Result.Failure<StockTraceDto>(Error.NotFound("StockItem", request.StockItemId));

        var events = new List<StockTraceEventDto>();

        // ── Receipt (creation) ────────────────────────────────────────────
        events.Add(new StockTraceEventDto(
            EventType:           "Receipt",
            OccurredAt:          item.CreatedAt,
            Quantity:            item.QuantityOnHand + item.Issues.Sum(i => i.QuantityIssued)
                                                     - item.Returns.Sum(r => r.QuantityReturned),
            QuantityOnHandAfter: null,
            WorkOrderId:         null,
            WorkOrderTaskId:     null,
            Reference:           null,
            BatchNumber:         item.BatchNumber,
            SerialNumber:        item.SerialNumber,
            ActorId:             item.CreatedBy));

        // ── Reservations ──────────────────────────────────────────────────
        foreach (var res in item.Reservations.OrderBy(r => r.ReservedAt))
        {
            events.Add(new StockTraceEventDto(
                EventType:           "Reservation",
                OccurredAt:          res.ReservedAt,
                Quantity:            res.QuantityReserved,
                QuantityOnHandAfter: null,
                WorkOrderId:         res.WorkOrderId,
                WorkOrderTaskId:     res.WorkOrderTaskId,
                Reference:           res.Id.ToString(),
                BatchNumber:         null,
                SerialNumber:        null,
                ActorId:             res.CreatedBy));
        }

        // ── Issues ────────────────────────────────────────────────────────
        foreach (var iss in item.Issues.OrderBy(i => i.IssuedAt))
        {
            events.Add(new StockTraceEventDto(
                EventType:           "Issue",
                OccurredAt:          iss.IssuedAt,
                Quantity:            -iss.QuantityIssued,
                QuantityOnHandAfter: null,
                WorkOrderId:         iss.WorkOrderId,
                WorkOrderTaskId:     iss.WorkOrderTaskId,
                Reference:           iss.IssueSlipRef,
                BatchNumber:         iss.BatchNumber,
                SerialNumber:        iss.SerialNumber,
                ActorId:             iss.IssuedByUserId));
        }

        // ── Returns ───────────────────────────────────────────────────────
        foreach (var ret in item.Returns.OrderBy(r => r.ReturnedAt))
        {
            events.Add(new StockTraceEventDto(
                EventType:           "Return",
                OccurredAt:          ret.ReturnedAt,
                Quantity:            ret.QuantityReturned,
                QuantityOnHandAfter: null,
                WorkOrderId:         ret.WorkOrderId,
                WorkOrderTaskId:     ret.WorkOrderTaskId,
                Reference:           ret.Reason,
                BatchNumber:         ret.BatchNumber,
                SerialNumber:        ret.SerialNumber,
                ActorId:             ret.ReturnedByUserId));
        }

        var dto = new StockTraceDto(
            item.Id,
            item.PartId,
            item.BatchNumber,
            item.SerialNumber,
            item.ExpiresAt,
            events.OrderBy(e => e.OccurredAt).ToList());

        return Result.Success(dto);
    }
}
