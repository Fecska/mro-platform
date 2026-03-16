using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Aircraft.Commands;
using Mro.Application.Features.Aircraft.Queries;
using Mro.Domain.Aggregates.Aircraft.Enums;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/aircraft")]
[Authorize]
public sealed class AircraftController(ISender sender) : ControllerBase
{
    // ── GET /api/aircraft ─────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListAircraftQuery(page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/aircraft ────────────────────────────────────────────────

    public sealed record RegisterAircraftRequest(
        string Registration,
        string SerialNumber,
        Guid AircraftTypeId,
        DateOnly ManufactureDate,
        string? Remarks);

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterAircraftRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RegisterAircraftCommand
        {
            Registration    = request.Registration,
            SerialNumber    = request.SerialNumber,
            AircraftTypeId  = request.AircraftTypeId,
            ManufactureDate = request.ManufactureDate,
            Remarks         = request.Remarks,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/aircraft/{id} ────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetAircraftQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── PATCH /api/aircraft/{id} ──────────────────────────────────────────

    public sealed record PatchAircraftRequest(string? Remarks);

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PatchAircraftRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateAircraftCommand
        {
            AircraftId    = id,
            Remarks       = request.Remarks,
            UpdateRemarks = true,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/aircraft/{id}/counters ───────────────────────────────────

    [HttpGet("{id:guid}/counters")]
    public async Task<IActionResult> GetCounters(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetCountersQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/aircraft/{id}/counters ──────────────────────────────────

    [HttpPost("{id:guid}/counters")]
    public async Task<IActionResult> UpdateCounters(
        Guid id,
        [FromBody] IReadOnlyList<CounterUpdate> updates,
        CancellationToken ct)
    {
        var result = await sender.Send(new UpdateCountersCommand { AircraftId = id, Updates = updates }, ct);
        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/aircraft/{id}/status-history ─────────────────────────────

    [HttpGet("{id:guid}/status-history")]
    public async Task<IActionResult> GetStatusHistory(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetStatusHistoryQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/aircraft/{id}/components ─────────────────────────────────

    [HttpGet("{id:guid}/components")]
    public async Task<IActionResult> ListComponents(
        Guid id,
        [FromQuery] bool installedOnly = true,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListComponentsQuery(id, installedOnly), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/aircraft/{id}/components/install ────────────────────────

    public sealed record InstallComponentRequest(
        string PartNumber,
        string SerialNumber,
        string Description,
        string InstallationPosition,
        Guid? WorkOrderId,
        Guid? InventoryItemId);

    [HttpPost("{id:guid}/components/install")]
    public async Task<IActionResult> InstallComponent(
        Guid id,
        [FromBody] InstallComponentRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new InstallComponentCommand
        {
            AircraftId           = id,
            PartNumber           = request.PartNumber,
            SerialNumber         = request.SerialNumber,
            Description          = request.Description,
            InstallationPosition = request.InstallationPosition,
            WorkOrderId          = request.WorkOrderId,
            InventoryItemId      = request.InventoryItemId,
        }, ct);

        return result.IsSuccess ? Ok(new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/aircraft/{id}/components/remove ─────────────────────────

    public sealed record RemoveComponentRequest(Guid ComponentId, string Reason, Guid? WorkOrderId);

    [HttpPost("{id:guid}/components/remove")]
    public async Task<IActionResult> RemoveComponent(
        Guid id,
        [FromBody] RemoveComponentRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new RemoveComponentCommand
        {
            AircraftId  = id,
            ComponentId = request.ComponentId,
            Reason      = request.Reason,
            WorkOrderId = request.WorkOrderId,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/aircraft/{id}/status ────────────────────────────────────

    public sealed record ChangeStatusRequest(AircraftStatus NewStatus, string Reason);

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        [FromBody] ChangeStatusRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new ChangeAircraftStatusCommand
        {
            AircraftId = id,
            NewStatus  = request.NewStatus,
            Reason     = request.Reason,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }
}
