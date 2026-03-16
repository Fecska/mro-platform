using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Inspections.Commands;
using Mro.Application.Features.Inspections.Queries;
using Mro.Domain.Aggregates.Inspection.Enums;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/inspections")]
[Authorize]
public sealed class InspectionsController(ISender sender) : ControllerBase
{
    // ── GET /api/inspections ───────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? workOrderId,
        [FromQuery] InspectionStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListInspectionsQuery(workOrderId, status, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/inspections/{id} ──────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetInspectionQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/inspections ──────────────────────────────────────────────

    public sealed record CreateInspectionRequest(
        Guid WorkOrderId,
        Guid AircraftId,
        InspectionType InspectionType,
        Guid InspectorUserId,
        Guid? WorkOrderTaskId,
        DateTimeOffset? ScheduledAt);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInspectionRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateInspectionCommand
        {
            WorkOrderId      = request.WorkOrderId,
            AircraftId       = request.AircraftId,
            InspectionType   = request.InspectionType,
            InspectorUserId  = request.InspectorUserId,
            WorkOrderTaskId  = request.WorkOrderTaskId,
            ScheduledAt      = request.ScheduledAt,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/inspections/{id}/start ──────────────────────────────────

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new StartInspectionCommand { InspectionId = id }, ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/inspections/{id}/outcome ────────────────────────────────

    public sealed record RecordOutcomeRequest(bool Passed, string Remarks, string? Findings);

    [HttpPost("{id:guid}/outcome")]
    public async Task<IActionResult> RecordOutcome(Guid id, [FromBody] RecordOutcomeRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RecordInspectionOutcomeCommand
        {
            InspectionId = id,
            Passed       = request.Passed,
            Remarks      = request.Remarks,
            Findings     = request.Findings,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/inspections/{id}/waive ──────────────────────────────────

    public sealed record WaiveRequest(string Reason);

    [HttpPost("{id:guid}/waive")]
    public async Task<IActionResult> Waive(Guid id, [FromBody] WaiveRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new WaiveInspectionCommand
        {
            InspectionId = id,
            Reason       = request.Reason,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }
}
