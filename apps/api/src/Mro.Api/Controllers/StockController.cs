using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Inventory.Commands;
using Mro.Application.Features.Inventory.Queries;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/stock")]
[Authorize]
public sealed class StockController(ISender sender) : ControllerBase
{
    // ── GET /api/stock ─────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? partId,
        [FromQuery] Guid? binLocationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListStockItemsQuery(partId, binLocationId, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/stock/{id} ────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetStockItemQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/stock/receive ────────────────────────────────────────────

    public sealed record ReceiveStockRequest(
        Guid PartId,
        Guid BinLocationId,
        decimal Quantity,
        decimal UnitCost,
        string? BatchNumber,
        string? SerialNumber,
        DateTimeOffset? ExpiresAt);

    [HttpPost("receive")]
    public async Task<IActionResult> Receive([FromBody] ReceiveStockRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ReceiveStockCommand
        {
            PartId        = request.PartId,
            BinLocationId = request.BinLocationId,
            Quantity      = request.Quantity,
            UnitCost      = request.UnitCost,
            BatchNumber   = request.BatchNumber,
            SerialNumber  = request.SerialNumber,
            ExpiresAt     = request.ExpiresAt,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/stock/{id}/reserve ───────────────────────────────────────

    public sealed record ReserveRequest(decimal Quantity, Guid WorkOrderId, Guid WorkOrderTaskId);

    [HttpPost("{id:guid}/reserve")]
    public async Task<IActionResult> Reserve(Guid id, [FromBody] ReserveRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ReserveStockCommand
        {
            StockItemId     = id,
            Quantity        = request.Quantity,
            WorkOrderId     = request.WorkOrderId,
            WorkOrderTaskId = request.WorkOrderTaskId,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/stock/{id}/issue ─────────────────────────────────────────

    public sealed record IssueRequest(
        decimal Quantity,
        Guid WorkOrderId,
        Guid WorkOrderTaskId,
        Guid? ReservationId,
        string? BatchNumber,
        string? SerialNumber,
        string? IssueSlipRef);

    [HttpPost("{id:guid}/issue")]
    public async Task<IActionResult> Issue(Guid id, [FromBody] IssueRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new IssueStockCommand
        {
            StockItemId     = id,
            Quantity        = request.Quantity,
            WorkOrderId     = request.WorkOrderId,
            WorkOrderTaskId = request.WorkOrderTaskId,
            ReservationId   = request.ReservationId,
            BatchNumber     = request.BatchNumber,
            SerialNumber    = request.SerialNumber,
            IssueSlipRef    = request.IssueSlipRef,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/stock/{id}/adjust ────────────────────────────────────────

    public sealed record AdjustRequest(decimal Delta, string Reason);

    [HttpPost("{id:guid}/adjust")]
    public async Task<IActionResult> Adjust(Guid id, [FromBody] AdjustRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AdjustStockCommand
        {
            StockItemId = id,
            Delta       = request.Delta,
            Reason      = request.Reason,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/stock/bin-locations ───────────────────────────────────────

    [HttpGet("bin-locations")]
    public async Task<IActionResult> ListBinLocations(CancellationToken ct)
    {
        var result = await sender.Send(new ListBinLocationsQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/stock/bin-locations ──────────────────────────────────────

    public sealed record CreateBinLocationRequest(string Code, string? Description, string? StoreRoom);

    [HttpPost("bin-locations")]
    public async Task<IActionResult> CreateBinLocation([FromBody] CreateBinLocationRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateBinLocationCommand
        {
            Code        = request.Code,
            Description = request.Description,
            StoreRoom   = request.StoreRoom,
        }, ct);

        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }
}
