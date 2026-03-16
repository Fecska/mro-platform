using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Inventory.Commands;
using Mro.Application.Features.Inventory.Queries;
using Mro.Domain.Aggregates.Inventory.Enums;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/parts")]
[Authorize]
public sealed class PartsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] PartStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListPartsQuery(status, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetPartQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record CreatePartRequest(
        string PartNumber,
        string Description,
        string UnitOfMeasure,
        string? AtaChapter,
        string? Manufacturer,
        string? ManufacturerPartNumber,
        bool TraceabilityRequired,
        decimal MinStockLevel);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreatePartCommand
        {
            PartNumber             = request.PartNumber,
            Description            = request.Description,
            UnitOfMeasure          = request.UnitOfMeasure,
            AtaChapter             = request.AtaChapter,
            Manufacturer           = request.Manufacturer,
            ManufacturerPartNumber = request.ManufacturerPartNumber,
            TraceabilityRequired   = request.TraceabilityRequired,
            MinStockLevel          = request.MinStockLevel,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }
}
