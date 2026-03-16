using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.WorkOrders.Commands;
using Mro.Application.Features.WorkOrders.Queries;
using Mro.Domain.Aggregates.WorkOrder.Enums;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/work-orders")]
[Authorize]
public sealed class WorkOrdersController(ISender sender) : ControllerBase
{
    // ── GET /api/work-orders ───────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? aircraftId,
        [FromQuery] WorkOrderStatus? status,
        [FromQuery] WorkOrderType? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListWorkOrdersQuery(aircraftId, status, type, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/work-orders/{id} ──────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetWorkOrderQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders ──────────────────────────────────────────────

    public sealed record CreateWorkOrderRequest(
        WorkOrderType WorkOrderType,
        string Title,
        Guid AircraftId,
        Guid? StationId,
        DateTimeOffset? PlannedStartAt,
        DateTimeOffset? PlannedEndAt,
        string? CustomerOrderRef,
        Guid? OriginatingDefectId);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateWorkOrderCommand
        {
            WorkOrderType = request.WorkOrderType,
            Title = request.Title,
            AircraftId = request.AircraftId,
            StationId = request.StationId,
            PlannedStartAt = request.PlannedStartAt,
            PlannedEndAt = request.PlannedEndAt,
            CustomerOrderRef = request.CustomerOrderRef,
            OriginatingDefectId = request.OriginatingDefectId,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/status ─────────────────────────────────

    public sealed record ChangeStatusRequest(
        WorkOrderStatus NewStatus,
        string? CancellationReason,
        Guid? CertifyingStaffUserId);

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ChangeWorkOrderStatusCommand
        {
            WorkOrderId = id,
            NewStatus = request.NewStatus,
            CancellationReason = request.CancellationReason,
            CertifyingStaffUserId = request.CertifyingStaffUserId,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/tasks ──────────────────────────────────

    public sealed record AddTaskRequest(
        string Title,
        string AtaChapter,
        string Description,
        decimal EstimatedHours,
        string? RequiredLicence,
        Guid? DocumentId);

    [HttpPost("{id:guid}/tasks")]
    public async Task<IActionResult> AddTask(Guid id, [FromBody] AddTaskRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AddTaskCommand
        {
            WorkOrderId = id,
            Title = request.Title,
            AtaChapter = request.AtaChapter,
            Description = request.Description,
            EstimatedHours = request.EstimatedHours,
            RequiredLicence = request.RequiredLicence,
            DocumentId = request.DocumentId,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/tasks/{taskId}/action ──────────────────

    public sealed record TaskActionRequest(
        TaskAction Action,
        Guid? CertifyingStaffUserId,
        string? SignOffRemark);

    [HttpPost("{id:guid}/tasks/{taskId:guid}/action")]
    public async Task<IActionResult> TaskAction(
        Guid id, Guid taskId,
        [FromBody] TaskActionRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new TaskActionCommand
        {
            WorkOrderId = id,
            TaskId = taskId,
            Action = request.Action,
            CertifyingStaffUserId = request.CertifyingStaffUserId,
            SignOffRemark = request.SignOffRemark,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/assignments ────────────────────────────

    public sealed record AssignPersonnelRequest(Guid UserId, AssignmentRole Role);

    [HttpPost("{id:guid}/assignments")]
    public async Task<IActionResult> AssignPersonnel(Guid id, [FromBody] AssignPersonnelRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AssignPersonnelCommand
        {
            WorkOrderId = id,
            UserId = request.UserId,
            Role = request.Role,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/tasks/{taskId}/labour ──────────────────

    public sealed record LogLabourRequest(
        Guid PerformedByUserId,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        string? Notes);

    [HttpPost("{id:guid}/tasks/{taskId:guid}/labour")]
    public async Task<IActionResult> LogLabour(
        Guid id, Guid taskId,
        [FromBody] LogLabourRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new LogLabourCommand
        {
            WorkOrderId = id,
            TaskId = taskId,
            PerformedByUserId = request.PerformedByUserId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Notes = request.Notes,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/blockers ───────────────────────────────

    public sealed record RaiseBlockerRequest(
        BlockerType BlockerType,
        string Description,
        WorkOrderStatus WaitingStatus);

    [HttpPost("{id:guid}/blockers")]
    public async Task<IActionResult> RaiseBlocker(Guid id, [FromBody] RaiseBlockerRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RaiseBlockerCommand
        {
            WorkOrderId = id,
            BlockerType = request.BlockerType,
            Description = request.Description,
            WaitingStatus = request.WaitingStatus,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── DELETE /api/work-orders/{id}/blockers/{blockerId} ─────────────────

    public sealed record ResolveBlockerRequest(string ResolutionNote);

    [HttpDelete("{id:guid}/blockers/{blockerId:guid}")]
    public async Task<IActionResult> ResolveBlocker(
        Guid id, Guid blockerId,
        [FromBody] ResolveBlockerRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new ResolveBlockerCommand
        {
            WorkOrderId = id,
            BlockerId = blockerId,
            ResolutionNote = request.ResolutionNote,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── PATCH /api/work-orders/{id} ───────────────────────────────────────

    public sealed record PatchWorkOrderRequest(
        string? Title,
        Guid? StationId,
        bool ClearStation,
        DateTimeOffset? PlannedStartAt,
        DateTimeOffset? PlannedEndAt,
        string? CustomerOrderRef);

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PatchWorkOrderRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateWorkOrderCommand
        {
            WorkOrderId      = id,
            Title            = request.Title,
            StationId        = request.StationId,
            ClearStation     = request.ClearStation,
            PlannedStartAt   = request.PlannedStartAt,
            PlannedEndAt     = request.PlannedEndAt,
            CustomerOrderRef = request.CustomerOrderRef,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/issue ──────────────────────────────────

    [HttpPost("{id:guid}/issue")]
    public async Task<IActionResult> Issue(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new IssueWorkOrderCommand { WorkOrderId = id }, ct);
        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/start ──────────────────────────────────

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new StartWorkOrderCommand { WorkOrderId = id }, ct);
        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/pause ──────────────────────────────────
    // Raises a blocker and transitions to WaitingParts or WaitingTooling.

    public sealed record PauseWorkOrderRequest(
        BlockerType BlockerType,
        string Description,
        WorkOrderStatus WaitingStatus);

    [HttpPost("{id:guid}/pause")]
    public async Task<IActionResult> Pause(Guid id, [FromBody] PauseWorkOrderRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RaiseBlockerCommand
        {
            WorkOrderId   = id,
            BlockerType   = request.BlockerType,
            Description   = request.Description,
            WaitingStatus = request.WaitingStatus,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/complete ───────────────────────────────

    public sealed record CompleteWorkOrderRequest(Guid CertifyingStaffUserId);

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteWorkOrderRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CompleteWorkOrderCommand
        {
            WorkOrderId            = id,
            CertifyingStaffUserId  = request.CertifyingStaffUserId,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/close ──────────────────────────────────

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new CloseWorkOrderCommand { WorkOrderId = id }, ct);
        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/assign ─────────────────────────────────

    public sealed record AssignRequest(Guid UserId, AssignmentRole Role);

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AssignPersonnelCommand
        {
            WorkOrderId = id,
            UserId      = request.UserId,
            Role        = request.Role,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/labour ─────────────────────────────────

    public sealed record WoLogLabourRequest(
        Guid TaskId,
        Guid PerformedByUserId,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        string? Notes);

    [HttpPost("{id:guid}/labour")]
    public async Task<IActionResult> Labour(Guid id, [FromBody] WoLogLabourRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new LogLabourCommand
        {
            WorkOrderId       = id,
            TaskId            = request.TaskId,
            PerformedByUserId = request.PerformedByUserId,
            StartAt           = request.StartAt,
            EndAt             = request.EndAt,
            Notes             = request.Notes,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/work-orders/{id}/blockers ────────────────────────────────

    [HttpGet("{id:guid}/blockers")]
    public async Task<IActionResult> ListBlockers(
        Guid id,
        [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListBlockersQuery(id, activeOnly), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/request-release ────────────────────────
    // Submits the WO for CRS certification (→ WaitingCertification).
    // HS-009b: all active tasks must be SignedOff first.

    [HttpPost("{id:guid}/request-release")]
    public async Task<IActionResult> RequestRelease(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new RequestReleaseCommand { WorkOrderId = id }, ct);
        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/work-orders/{id}/sign-off ───────────────────────────────
    // Certifying engineer issues the CRS and completes the work order.

    [HttpPost("{id:guid}/sign-off")]
    public async Task<IActionResult> SignOff(Guid id, [FromBody] CompleteWorkOrderRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CompleteWorkOrderCommand
        {
            WorkOrderId           = id,
            CertifyingStaffUserId = request.CertifyingStaffUserId,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/work-orders/{id}/eligible-signers ────────────────────────

    [HttpGet("{id:guid}/eligible-signers")]
    public async Task<IActionResult> EligibleSigners(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetEligibleSignersQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/work-orders/{id}/prerequisites ────────────────────────────
    // Blocker engine: detects missing documents, missing authorisations,
    // expired tool calibrations, and unchecked-out tools.

    [HttpGet("{id:guid}/prerequisites")]
    public async Task<IActionResult> Prerequisites(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new CheckPrerequisitesQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }
}
