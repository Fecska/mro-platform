using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Maintenance.Commands;
using Mro.Application.Features.Maintenance.Queries;
using Mro.Domain.Aggregates.Maintenance.Enums;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/due-items")]
[Authorize]
public sealed class DueItemsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? aircraftId,
        [FromQuery] DueStatus? status,
        [FromQuery] DueItemType? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListDueItemsQuery(aircraftId, status, type, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetDueItemQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record CreateDueItemRequest(
        string DueItemRef,
        Guid AircraftId,
        DueItemType DueItemType,
        IntervalType IntervalType,
        string Description,
        Guid? MaintenanceProgramId,
        string? RegulatoryRef,
        decimal? IntervalValue,
        int? IntervalDays,
        decimal? ToleranceValue,
        DateTimeOffset? NextDueDate,
        decimal? NextDueHours,
        int? NextDueCycles);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDueItemRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateDueItemCommand
        {
            DueItemRef           = request.DueItemRef,
            AircraftId           = request.AircraftId,
            DueItemType          = request.DueItemType,
            IntervalType         = request.IntervalType,
            Description          = request.Description,
            MaintenanceProgramId = request.MaintenanceProgramId,
            RegulatoryRef        = request.RegulatoryRef,
            IntervalValue        = request.IntervalValue,
            IntervalDays         = request.IntervalDays,
            ToleranceValue       = request.ToleranceValue,
            NextDueDate          = request.NextDueDate,
            NextDueHours         = request.NextDueHours,
            NextDueCycles        = request.NextDueCycles,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record RecordAccomplishmentRequest(
        Guid WorkOrderId,
        DateTimeOffset AccomplishedAt,
        decimal? AtHours,
        int? AtCycles);

    [HttpPost("{id:guid}/accomplish")]
    public async Task<IActionResult> Accomplish(Guid id, [FromBody] RecordAccomplishmentRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RecordAccomplishmentCommand
        {
            DueItemId       = id,
            WorkOrderId     = request.WorkOrderId,
            AccomplishedAt  = request.AccomplishedAt,
            AtHours         = request.AtHours,
            AtCycles        = request.AtCycles,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record DeferRequest(DateTimeOffset NewDueDate, string Justification);

    [HttpPost("{id:guid}/defer")]
    public async Task<IActionResult> Defer(Guid id, [FromBody] DeferRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new DeferDueItemCommand
        {
            DueItemId    = id,
            NewDueDate   = request.NewDueDate,
            Justification = request.Justification,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }
}
