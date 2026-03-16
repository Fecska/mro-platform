using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Inventory.Queries;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/stock-items")]
[Authorize]
public sealed class StockItemsController(ISender sender) : ControllerBase
{
    // ── GET /api/stock-items ───────────────────────────────────────────────

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

    // ── GET /api/stock-items/{id}/trace ────────────────────────────────────

    [HttpGet("{id:guid}/trace")]
    public async Task<IActionResult> Trace(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetStockTraceQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }
}
