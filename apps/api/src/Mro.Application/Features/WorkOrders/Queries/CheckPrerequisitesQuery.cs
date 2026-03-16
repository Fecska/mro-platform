using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.WorkOrder.Enums;

namespace Mro.Application.Features.WorkOrders.Queries;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public enum PrerequisiteIssueKind
{
    /// <summary>A task references a document that is missing or deleted.</summary>
    MissingDocument,

    /// <summary>
    /// A task requires a specific licence category but no Certifying Staff
    /// with that category is assigned to the work order.
    /// </summary>
    MissingAuthorisation,

    /// <summary>A task's required tool has an expired calibration.</summary>
    ExpiredToolCalibration,

    /// <summary>A task's required tool is not yet checked out from the tool store.</summary>
    ToolNotCheckedOut,
}

public sealed record PrerequisiteIssueDto(
    PrerequisiteIssueKind Kind,
    /// <summary>Null when the issue is at WO level rather than task level.</summary>
    Guid? TaskId,
    string? TaskNumber,
    string Message);

public sealed record PrerequisiteCheckDto(
    Guid WorkOrderId,
    string WoNumber,
    bool IsReadyToStart,
    IReadOnlyList<PrerequisiteIssueDto> Issues);

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record CheckPrerequisitesQuery(Guid WorkOrderId)
    : IRequest<Result<PrerequisiteCheckDto>>;

public sealed class CheckPrerequisitesQueryHandler(
    IWorkOrderRepository workOrders,
    IDocumentRepository documents,
    ICurrentUserService currentUser)
    : IRequestHandler<CheckPrerequisitesQuery, Result<PrerequisiteCheckDto>>
{
    public async Task<Result<PrerequisiteCheckDto>> Handle(
        CheckPrerequisitesQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<PrerequisiteCheckDto>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, orgId, ct);
        if (wo is null)
            return Result.Failure<PrerequisiteCheckDto>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var issues = new List<PrerequisiteIssueDto>();
        var activeTasks = wo.Tasks
            .Where(t => t.Status != WorkOrderTaskStatus.Cancelled)
            .ToList();

        // ── 1. Missing / deleted document ─────────────────────────────────
        foreach (var task in activeTasks.Where(t => t.DocumentId.HasValue))
        {
            var doc = await documents.GetByIdAsync(task.DocumentId!.Value, orgId, ct);
            if (doc is null)
            {
                issues.Add(new PrerequisiteIssueDto(
                    PrerequisiteIssueKind.MissingDocument,
                    task.Id,
                    task.TaskNumber,
                    $"Task {task.TaskNumber}: referenced document {task.DocumentId} not found or deleted."));
            }
        }

        // ── 2. Missing authorisation ──────────────────────────────────────
        // For each task that specifies a RequiredLicence, verify that at least
        // one Certifying Staff member is assigned to the work order.
        // (First version: role-level check; future version can validate specific category.)
        var hasCertifyingStaff = wo.Assignments.Any(a => a.Role == AssignmentRole.CertifyingStaff);

        foreach (var task in activeTasks.Where(t => !string.IsNullOrWhiteSpace(t.RequiredLicence)))
        {
            if (!hasCertifyingStaff)
            {
                issues.Add(new PrerequisiteIssueDto(
                    PrerequisiteIssueKind.MissingAuthorisation,
                    task.Id,
                    task.TaskNumber,
                    $"Task {task.TaskNumber}: requires licence '{task.RequiredLicence}' " +
                    $"but no Certifying Staff is assigned to work order '{wo.WoNumber}'."));
            }
        }

        // ── 3. Expired tool calibration ───────────────────────────────────
        foreach (var task in activeTasks)
        {
            foreach (var tool in task.RequiredTools.Where(t => t.IsCalibrationExpired))
            {
                issues.Add(new PrerequisiteIssueDto(
                    PrerequisiteIssueKind.ExpiredToolCalibration,
                    task.Id,
                    task.TaskNumber,
                    $"Task {task.TaskNumber}: tool '{tool.ToolNumber}' calibration expired " +
                    $"({tool.CalibratedExpiry:yyyy-MM-dd}). Hard Stop HS-010."));
            }
        }

        // ── 4. Tool not yet checked out ────────────────────────────────────
        foreach (var task in activeTasks.Where(t => t.Status == WorkOrderTaskStatus.Pending
                                                  || t.Status == WorkOrderTaskStatus.InProgress))
        {
            foreach (var tool in task.RequiredTools.Where(t => !t.IsCheckedOut && !t.IsCalibrationExpired))
            {
                issues.Add(new PrerequisiteIssueDto(
                    PrerequisiteIssueKind.ToolNotCheckedOut,
                    task.Id,
                    task.TaskNumber,
                    $"Task {task.TaskNumber}: tool '{tool.ToolNumber}' is required but not yet " +
                    $"checked out from the tool store."));
            }
        }

        var dto = new PrerequisiteCheckDto(
            wo.Id,
            wo.WoNumber,
            IsReadyToStart: !issues.Any(i =>
                i.Kind is PrerequisiteIssueKind.MissingDocument
                       or PrerequisiteIssueKind.MissingAuthorisation
                       or PrerequisiteIssueKind.ExpiredToolCalibration),
            issues);

        return Result.Success(dto);
    }
}
