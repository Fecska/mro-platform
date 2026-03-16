using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Inventory.Commands;

namespace Mro.Api.Controllers;

// ── POST /api/material-reservations ───────────────────────────────────────────

[ApiController]
[Route("api/material-reservations")]
[Authorize]
public sealed class MaterialReservationsController(ISender sender) : ControllerBase
{
    public sealed record CreateReservationRequest(
        Guid StockItemId,
        decimal Quantity,
        Guid WorkOrderId,
        Guid WorkOrderTaskId);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ReserveStockCommand
        {
            StockItemId     = request.StockItemId,
            Quantity        = request.Quantity,
            WorkOrderId     = request.WorkOrderId,
            WorkOrderTaskId = request.WorkOrderTaskId,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }
}

// ── POST /api/material-issues ─────────────────────────────────────────────────

[ApiController]
[Route("api/material-issues")]
[Authorize]
public sealed class MaterialIssuesController(ISender sender) : ControllerBase
{
    public sealed record CreateIssueRequest(
        Guid StockItemId,
        decimal Quantity,
        Guid WorkOrderId,
        Guid WorkOrderTaskId,
        Guid? ReservationId,
        string? BatchNumber,
        string? SerialNumber,
        string? IssueSlipRef);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIssueRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new IssueStockCommand
        {
            StockItemId     = request.StockItemId,
            Quantity        = request.Quantity,
            WorkOrderId     = request.WorkOrderId,
            WorkOrderTaskId = request.WorkOrderTaskId,
            ReservationId   = request.ReservationId,
            BatchNumber     = request.BatchNumber,
            SerialNumber    = request.SerialNumber,
            IssueSlipRef    = request.IssueSlipRef,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }
}

// ── POST /api/material-returns ────────────────────────────────────────────────

[ApiController]
[Route("api/material-returns")]
[Authorize]
public sealed class MaterialReturnsController(ISender sender) : ControllerBase
{
    public sealed record CreateReturnRequest(
        Guid StockItemId,
        decimal Quantity,
        Guid WorkOrderId,
        Guid WorkOrderTaskId,
        string Reason,
        Guid? OriginalIssueId,
        string? BatchNumber,
        string? SerialNumber);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReturnRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ReturnMaterialCommand
        {
            StockItemId     = request.StockItemId,
            Quantity        = request.Quantity,
            WorkOrderId     = request.WorkOrderId,
            WorkOrderTaskId = request.WorkOrderTaskId,
            Reason          = request.Reason,
            OriginalIssueId = request.OriginalIssueId,
            BatchNumber     = request.BatchNumber,
            SerialNumber    = request.SerialNumber,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }
}
