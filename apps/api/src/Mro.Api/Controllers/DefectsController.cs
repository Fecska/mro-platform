using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Defects.Commands;
using Mro.Application.Features.Defects.Queries;
using Mro.Domain.Aggregates.Defect.Enums;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/defects")]
[Authorize]
public sealed class DefectsController(ISender sender) : ControllerBase
{
    // ── GET /api/defects ───────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? aircraftId,
        [FromQuery] DefectStatus? status,
        [FromQuery] DefectSeverity? severity,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListDefectsQuery(aircraftId, status, severity, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/defects ──────────────────────────────────────────────────

    public sealed record RaiseDefectRequest(
        Guid AircraftId,
        DefectSeverity Severity,
        DefectSource Source,
        string AtaChapter,
        string Description,
        DateTimeOffset DiscoveredAt,
        Guid? DiscoveredAtStationId,
        bool IsAdMandated,
        Guid? AdDocumentId);

    [HttpPost]
    public async Task<IActionResult> Raise([FromBody] RaiseDefectRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RaiseDefectCommand
        {
            AircraftId            = request.AircraftId,
            Severity              = request.Severity,
            Source                = request.Source,
            AtaChapter            = request.AtaChapter,
            Description           = request.Description,
            DiscoveredAt          = request.DiscoveredAt,
            DiscoveredAtStationId = request.DiscoveredAtStationId,
            IsAdMandated          = request.IsAdMandated,
            AdDocumentId          = request.AdDocumentId,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/defects/{id} ──────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetDefectQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── PATCH /api/defects/{id} ────────────────────────────────────────────

    public sealed record PatchDefectRequest(
        string? Description,
        string? AtaChapter,
        DefectSeverity? Severity);

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PatchDefectRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateDefectCommand
        {
            DefectId    = id,
            Description = request.Description,
            AtaChapter  = request.AtaChapter,
            Severity    = request.Severity,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/defects/{id}/triage ─────────────────────────────────────

    public sealed record TriageDefectRequest(Guid AssignedToUserId, DefectSeverity? OverrideSeverity);

    [HttpPost("{id:guid}/triage")]
    public async Task<IActionResult> Triage(Guid id, [FromBody] TriageDefectRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new TriageDefectCommand
        {
            DefectId         = id,
            AssignedToUserId = request.AssignedToUserId,
            OverrideSeverity = request.OverrideSeverity,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/defects/{id}/defer ──────────────────────────────────────

    public sealed record DeferDefectRequest(
        DateTimeOffset DeferredFrom,
        DateTimeOffset DeferredUntil,
        Guid ApprovedByUserId,
        Guid SignedByUserId,
        string? LogReference,
        Guid? StationId,
        string MelItemNumber,
        string MelRevision,
        DeferralCategory DeferralCategory,
        int? OperatorIntervalDays,
        string? OperationalLimitations,
        string? MaintenanceProcedures);

    [HttpPost("{id:guid}/defer")]
    public async Task<IActionResult> Defer(Guid id, [FromBody] DeferDefectRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new DeferDefectCommand
        {
            DefectId               = id,
            DeferredFrom           = request.DeferredFrom,
            DeferredUntil          = request.DeferredUntil,
            ApprovedByUserId       = request.ApprovedByUserId,
            SignedByUserId         = request.SignedByUserId,
            LogReference           = request.LogReference,
            StationId              = request.StationId,
            MelItemNumber          = request.MelItemNumber,
            MelRevision            = request.MelRevision,
            DeferralCategory       = request.DeferralCategory,
            OperatorIntervalDays   = request.OperatorIntervalDays,
            OperationalLimitations = request.OperationalLimitations,
            MaintenanceProcedures  = request.MaintenanceProcedures,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/defects/{id}/actions ────────────────────────────────────

    public sealed record RecordActionRequest(
        ActionType ActionType,
        string Description,
        Guid PerformedByUserId,
        DateTimeOffset PerformedAt,
        string? AtaReference,
        string? PartNumber,
        string? SerialNumber,
        Guid? WorkOrderId);

    [HttpPost("{id:guid}/actions")]
    public async Task<IActionResult> RecordAction(Guid id, [FromBody] RecordActionRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RecordDefectActionCommand
        {
            DefectId          = id,
            ActionType        = request.ActionType,
            Description       = request.Description,
            PerformedByUserId = request.PerformedByUserId,
            PerformedAt       = request.PerformedAt,
            AtaReference      = request.AtaReference,
            PartNumber        = request.PartNumber,
            SerialNumber      = request.SerialNumber,
            WorkOrderId       = request.WorkOrderId,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/defects/{id}/close ──────────────────────────────────────

    public sealed record CloseDefectRequest(string Reason);

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseDefectRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CloseDefectCommand
        {
            DefectId = id,
            Reason   = request.Reason,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/defects/{id}/create-work-order ──────────────────────────

    public sealed record CreateWorkOrderRequest(
        string Title,
        Guid? StationId,
        DateTimeOffset? PlannedStartAt,
        DateTimeOffset? PlannedEndAt);

    [HttpPost("{id:guid}/create-work-order")]
    public async Task<IActionResult> CreateWorkOrder(Guid id, [FromBody] CreateWorkOrderRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateDefectWorkOrderCommand
        {
            DefectId       = id,
            Title          = request.Title,
            StationId      = request.StationId,
            PlannedStartAt = request.PlannedStartAt,
            PlannedEndAt   = request.PlannedEndAt,
        }, ct);

        return result.IsSuccess
            ? Ok(new { workOrderId = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/defects/{id}/status ─────────────────────────────────────
    // Generic status transition (Accept, StartRectification, SubmitForInspection, Clear)

    public sealed record ChangeStatusRequest(
        DefectStatus NewStatus,
        Guid? WorkOrderId,
        Guid? CertifyingStaffUserId,
        string? ClosureReason);

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ChangeDefectStatusCommand
        {
            DefectId              = id,
            NewStatus             = request.NewStatus,
            WorkOrderId           = request.WorkOrderId,
            CertifyingStaffUserId = request.CertifyingStaffUserId,
            ClosureReason         = request.ClosureReason,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }
}
