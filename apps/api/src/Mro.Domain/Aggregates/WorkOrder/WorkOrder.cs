using Mro.Domain.Aggregates.WorkOrder.Enums;
using Mro.Domain.Aggregates.WorkOrder.Events;
using Mro.Domain.Application;
using Mro.Domain.Common.Audit;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.WorkOrder;

/// <summary>
/// Aggregate root for a Maintenance Work Order (WO).
///
/// Invariants:
///   - WO may not be Issued without at least one task (HS-009a).
///   - WO may not move to InProgress without at least one assignment.
///   - WO may not move to WaitingCertification unless all active tasks are SignedOff (HS-009b).
///   - WO may not be Completed unless it is in WaitingCertification.
///   - Active blockers must all be resolved before returning to InProgress.
///   - RequiredTool calibration expiry enforced by RequiredTool.CheckOut (HS-010).
///
/// State machine:
///   Draft → {Planned, Cancelled}
///   Planned → {Issued, Cancelled}
///   Issued → {InProgress, Cancelled}
///   InProgress → {WaitingParts, WaitingTooling, WaitingInspection, Cancelled}
///   WaitingParts → {InProgress, WaitingTooling, WaitingInspection, Cancelled}
///   WaitingTooling → {InProgress, WaitingParts, WaitingInspection, Cancelled}
///   WaitingInspection → {WaitingCertification, InProgress, Cancelled}
///   WaitingCertification → {Completed, WaitingInspection, Cancelled}
///   Completed → {Closed}
///   Closed → {}
///   Cancelled → {}
/// </summary>
public sealed class WorkOrder : AuditableEntity
{
    /// <summary>System-generated work order number (e.g. "WO-2025-00042").</summary>
    public string WoNumber { get; private set; } = string.Empty;

    public WorkOrderType WorkOrderType { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public WorkOrderStatus Status { get; private set; } = WorkOrderStatus.Draft;

    /// <summary>Aircraft this work is being performed on.</summary>
    public Guid AircraftId { get; private set; }

    /// <summary>Station where the work is being performed (cross-module FK).</summary>
    public Guid? StationId { get; private set; }

    public DateTimeOffset? PlannedStartAt { get; private set; }

    public DateTimeOffset? PlannedEndAt { get; private set; }

    public DateTimeOffset? ActualStartAt { get; private set; }

    public DateTimeOffset? ActualEndAt { get; private set; }

    /// <summary>Customer / operator purchase order reference, if applicable.</summary>
    public string? CustomerOrderRef { get; private set; }

    /// <summary>Optional reference to the defect that originated this WO (cross-module FK).</summary>
    public Guid? OriginatingDefectId { get; private set; }

    private readonly List<WorkOrderTask> _tasks = [];
    private readonly List<WorkOrderAssignment> _assignments = [];
    private readonly List<WorkOrderBlocker> _blockers = [];

    public IReadOnlyCollection<WorkOrderTask> Tasks => _tasks.AsReadOnly();
    public IReadOnlyCollection<WorkOrderAssignment> Assignments => _assignments.AsReadOnly();
    public IReadOnlyCollection<WorkOrderBlocker> Blockers => _blockers.AsReadOnly();

    public IReadOnlyCollection<WorkOrderBlocker> ActiveBlockers =>
        _blockers.Where(b => !b.IsResolved).ToList().AsReadOnly();

    public bool AllTasksSignedOff =>
        _tasks.Where(t => t.Status != WorkOrderTaskStatus.Cancelled).All(t => t.IsSignedOff);

    // EF Core
    private WorkOrder() { }

    // ── State machine ────────────────────────────────────────────────────────

    private static readonly IReadOnlyDictionary<WorkOrderStatus, IReadOnlySet<WorkOrderStatus>> AllowedTransitions =
        new Dictionary<WorkOrderStatus, IReadOnlySet<WorkOrderStatus>>
        {
            [WorkOrderStatus.Draft]                 = new HashSet<WorkOrderStatus> { WorkOrderStatus.Planned, WorkOrderStatus.Cancelled },
            [WorkOrderStatus.Planned]               = new HashSet<WorkOrderStatus> { WorkOrderStatus.Issued, WorkOrderStatus.Cancelled },
            [WorkOrderStatus.Issued]                = new HashSet<WorkOrderStatus> { WorkOrderStatus.InProgress, WorkOrderStatus.Cancelled },
            [WorkOrderStatus.InProgress]            = new HashSet<WorkOrderStatus> { WorkOrderStatus.WaitingParts, WorkOrderStatus.WaitingTooling, WorkOrderStatus.WaitingInspection, WorkOrderStatus.Cancelled },
            [WorkOrderStatus.WaitingParts]          = new HashSet<WorkOrderStatus> { WorkOrderStatus.InProgress, WorkOrderStatus.WaitingTooling, WorkOrderStatus.WaitingInspection, WorkOrderStatus.Cancelled },
            [WorkOrderStatus.WaitingTooling]        = new HashSet<WorkOrderStatus> { WorkOrderStatus.InProgress, WorkOrderStatus.WaitingParts, WorkOrderStatus.WaitingInspection, WorkOrderStatus.Cancelled },
            [WorkOrderStatus.WaitingInspection]     = new HashSet<WorkOrderStatus> { WorkOrderStatus.WaitingCertification, WorkOrderStatus.InProgress, WorkOrderStatus.Cancelled },
            [WorkOrderStatus.WaitingCertification]  = new HashSet<WorkOrderStatus> { WorkOrderStatus.Completed, WorkOrderStatus.WaitingInspection, WorkOrderStatus.Cancelled },
            [WorkOrderStatus.Completed]             = new HashSet<WorkOrderStatus> { WorkOrderStatus.Closed },
            [WorkOrderStatus.Closed]                = new HashSet<WorkOrderStatus>(),
            [WorkOrderStatus.Cancelled]             = new HashSet<WorkOrderStatus>(),
        };

    private DomainResult SetStatus(WorkOrderStatus newStatus, Guid actorId, string description)
    {
        if (!AllowedTransitions[Status].Contains(newStatus))
            return DomainResult.Failure(
                $"Transition from '{Status}' to '{newStatus}' is not permitted.");

        var from = Status;
        Status = newStatus;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new WorkOrderStatusChangedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(WorkOrder),
            EntityId = Id,
            EventType = ComplianceEventType.RecordUpdated,
            WoNumber = WoNumber,
            FromStatus = from,
            ToStatus = newStatus,
            Description = description,
        });

        return DomainResult.Ok();
    }

    // ── Factory ──────────────────────────────────────────────────────────────

    public static WorkOrder Create(
        string woNumber,
        WorkOrderType workOrderType,
        string title,
        Guid aircraftId,
        Guid organisationId,
        Guid actorId,
        Guid? stationId = null,
        DateTimeOffset? plannedStartAt = null,
        DateTimeOffset? plannedEndAt = null,
        string? customerOrderRef = null,
        Guid? originatingDefectId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(woNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var wo = new WorkOrder
        {
            WoNumber = woNumber.Trim().ToUpperInvariant(),
            WorkOrderType = workOrderType,
            Title = title.Trim(),
            AircraftId = aircraftId,
            StationId = stationId,
            PlannedStartAt = plannedStartAt,
            PlannedEndAt = plannedEndAt,
            CustomerOrderRef = customerOrderRef?.Trim(),
            OriginatingDefectId = originatingDefectId,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        wo.RaiseDomainEvent(new WorkOrderCreatedEvent
        {
            ActorId = actorId,
            OrganisationId = organisationId,
            EntityType = nameof(WorkOrder),
            EntityId = wo.Id,
            EventType = ComplianceEventType.WorkOrderOpened,
            WoNumber = wo.WoNumber,
            AircraftId = aircraftId,
            WorkOrderType = workOrderType,
            Description = $"Work order '{wo.WoNumber}' created ({workOrderType}) for aircraft {aircraftId}: {title}",
        });

        return wo;
    }

    // ── Workflow transitions ──────────────────────────────────────────────────

    public DomainResult Plan(Guid actorId) =>
        SetStatus(WorkOrderStatus.Planned, actorId,
            $"Work order '{WoNumber}' moved to Planned.");

    /// <summary>
    /// Issues the work pack to the hangar.
    /// Hard Stop HS-009a: at least one task must exist.
    /// </summary>
    public DomainResult Issue(Guid actorId)
    {
        if (!_tasks.Any(t => t.Status != WorkOrderTaskStatus.Cancelled))
            return DomainResult.Failure(
                "Hard Stop HS-009a: Work order must have at least one active task before it can be issued.");

        return SetStatus(WorkOrderStatus.Issued, actorId,
            $"Work order '{WoNumber}' issued to hangar.");
    }

    /// <summary>Starts physical work. Requires at least one assignment.</summary>
    public DomainResult StartWork(Guid actorId)
    {
        if (!_assignments.Any())
            return DomainResult.Failure(
                "Work order must have at least one personnel assignment before work can start.");

        var result = SetStatus(WorkOrderStatus.InProgress, actorId,
            $"Work order '{WoNumber}' work started.");
        if (result.IsFailure) return result;

        ActualStartAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    public DomainResult RaiseBlocker(
        BlockerType blockerType,
        string description,
        Guid raisedByUserId,
        WorkOrderStatus waitingStatus,
        Guid actorId)
    {
        if (waitingStatus is not WorkOrderStatus.WaitingParts and not WorkOrderStatus.WaitingTooling)
            return DomainResult.Failure("Blocker status must be WaitingParts or WaitingTooling.");

        var blocker = WorkOrderBlocker.Create(Id, blockerType, description, raisedByUserId, OrganisationId, actorId);
        _blockers.Add(blocker);

        return SetStatus(waitingStatus, actorId,
            $"Work order '{WoNumber}' blocked ({blockerType}): {description}");
    }

    public DomainResult ResolveBlocker(
        Guid blockerId,
        string resolutionNote,
        Guid resolvedByUserId,
        Guid actorId)
    {
        var blocker = _blockers.FirstOrDefault(b => b.Id == blockerId);
        if (blocker is null)
            return DomainResult.Failure($"Blocker {blockerId} not found on work order '{WoNumber}'.");

        blocker.Resolve(resolutionNote, resolvedByUserId, actorId);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    /// <summary>Submits for inspection. All blockers must be resolved first.</summary>
    public DomainResult SubmitForInspection(Guid actorId)
    {
        if (ActiveBlockers.Any())
            return DomainResult.Failure(
                $"Work order '{WoNumber}' has {ActiveBlockers.Count} unresolved blocker(s). Resolve them before submitting for inspection.");

        return SetStatus(WorkOrderStatus.WaitingInspection, actorId,
            $"Work order '{WoNumber}' submitted for inspection.");
    }

    /// <summary>
    /// Submits for CRS sign-off after inspection passes.
    /// Hard Stop HS-009b: all active tasks must be SignedOff.
    /// </summary>
    public DomainResult SubmitForCertification(Guid actorId)
    {
        if (!AllTasksSignedOff)
            return DomainResult.Failure(
                "Hard Stop HS-009b: All active tasks must be signed off before the work order can proceed to certification.");

        return SetStatus(WorkOrderStatus.WaitingCertification, actorId,
            $"Work order '{WoNumber}' submitted for CRS certification.");
    }

    /// <summary>
    /// Completes the work order (issues CRS at WO level).
    /// All tasks must already be individually signed off (enforced by SubmitForCertification).
    /// </summary>
    public DomainResult Complete(Guid certifyingStaffUserId, Guid actorId)
    {
        var result = SetStatus(WorkOrderStatus.Completed, actorId,
            $"Work order '{WoNumber}' completed and CRS issued by user {certifyingStaffUserId}.");
        if (result.IsFailure) return result;

        ActualEndAt = DateTimeOffset.UtcNow;

        RaiseDomainEvent(new WorkOrderCompletedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(WorkOrder),
            EntityId = Id,
            EventType = ComplianceEventType.WorkOrderClosed,
            WoNumber = WoNumber,
            AircraftId = AircraftId,
            CertifyingStaffUserId = certifyingStaffUserId,
            Description = $"Work order '{WoNumber}' completed; CRS issued by certifying staff (user {certifyingStaffUserId}).",
        });

        return DomainResult.Ok();
    }

    public DomainResult Close(Guid actorId) =>
        SetStatus(WorkOrderStatus.Closed, actorId,
            $"Work order '{WoNumber}' administratively closed.");

    public DomainResult Cancel(string reason, Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var result = SetStatus(WorkOrderStatus.Cancelled, actorId,
            $"Work order '{WoNumber}' cancelled: {reason}.");
        if (result.IsFailure) return result;

        RaiseDomainEvent(new WorkOrderStatusChangedEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(WorkOrder),
            EntityId = Id,
            EventType = ComplianceEventType.WorkOrderCancelled,
            WoNumber = WoNumber,
            FromStatus = WorkOrderStatus.Cancelled, // already set by SetStatus above
            ToStatus = WorkOrderStatus.Cancelled,
            Description = $"Work order '{WoNumber}' cancelled: {reason}.",
        });

        return DomainResult.Ok();
    }

    // ── Update details ───────────────────────────────────────────────────────

    public DomainResult UpdateDetails(
        string? title,
        Guid? stationId,
        bool clearStation,
        DateTimeOffset? plannedStartAt,
        DateTimeOffset? plannedEndAt,
        string? customerOrderRef,
        Guid actorId)
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Closed or WorkOrderStatus.Cancelled)
            return DomainResult.Failure($"Cannot update a '{Status}' work order.");

        if (title is not null) Title = title.Trim();
        if (clearStation) StationId = null;
        else if (stationId.HasValue) StationId = stationId;
        if (plannedStartAt.HasValue) PlannedStartAt = plannedStartAt;
        if (plannedEndAt.HasValue) PlannedEndAt = plannedEndAt;
        if (customerOrderRef is not null) CustomerOrderRef = customerOrderRef.Trim();

        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Task management ───────────────────────────────────────────────────────

    public DomainResult AddTask(
        string title,
        string ataChapter,
        string description,
        decimal estimatedHours,
        Guid actorId,
        string? requiredLicence = null,
        Guid? documentId = null)
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Closed or WorkOrderStatus.Cancelled)
            return DomainResult.Failure($"Cannot add tasks to a '{Status}' work order.");

        var taskNumber = $"T{(_tasks.Count + 1):D2}";

        var task = WorkOrderTask.Create(
            Id, taskNumber, title, ataChapter, description,
            estimatedHours, OrganisationId, actorId, requiredLicence, documentId);

        _tasks.Add(task);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    public DomainResult StartTask(Guid taskId, Guid actorId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            return DomainResult.Failure($"Task {taskId} not found on work order '{WoNumber}'.");

        return task.StartWork(actorId);
    }

    public DomainResult CompleteTask(Guid taskId, Guid actorId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            return DomainResult.Failure($"Task {taskId} not found on work order '{WoNumber}'.");

        return task.CompleteWork(actorId);
    }

    public DomainResult SignOffTask(Guid taskId, Guid certifyingStaffUserId, string remark, Guid actorId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            return DomainResult.Failure($"Task {taskId} not found on work order '{WoNumber}'.");

        var result = task.SignOff(certifyingStaffUserId, remark, actorId);
        if (result.IsFailure) return result;

        RaiseDomainEvent(new TaskSignedOffEvent
        {
            ActorId = actorId,
            OrganisationId = OrganisationId,
            EntityType = nameof(WorkOrder),
            EntityId = Id,
            EventType = ComplianceEventType.TaskSigned,
            WoNumber = WoNumber,
            TaskNumber = task.TaskNumber,
            CertifyingStaffUserId = certifyingStaffUserId,
            Description = $"Task '{task.TaskNumber}' on WO '{WoNumber}' signed off by certifying staff (user {certifyingStaffUserId}).",
        });

        return DomainResult.Ok();
    }

    // ── Personnel assignments ─────────────────────────────────────────────────

    public DomainResult AssignPersonnel(
        Guid userId,
        AssignmentRole role,
        Guid assignedByUserId,
        Guid actorId)
    {
        if (Status is WorkOrderStatus.Completed or WorkOrderStatus.Closed or WorkOrderStatus.Cancelled)
            return DomainResult.Failure($"Cannot assign personnel to a '{Status}' work order.");

        var duplicate = _assignments.FirstOrDefault(
            a => a.UserId == userId && a.Role == role);
        if (duplicate is not null)
            return DomainResult.Failure(
                $"User {userId} is already assigned as {role} on work order '{WoNumber}'.");

        var assignment = WorkOrderAssignment.Create(
            Id, userId, role, assignedByUserId, OrganisationId, actorId);
        _assignments.Add(assignment);
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // ── Labour / Parts / Tools delegated through task ─────────────────────────

    public DomainResult LogLabour(
        Guid taskId,
        Guid performedByUserId,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        Guid actorId,
        string? notes = null)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            return DomainResult.Failure($"Task {taskId} not found.");

        return task.AddLabourEntry(performedByUserId, startAt, endAt, actorId, notes);
    }

    public DomainResult AddRequiredPart(
        Guid taskId,
        string partNumber,
        string description,
        decimal quantity,
        string unitOfMeasure,
        Guid actorId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            return DomainResult.Failure($"Task {taskId} not found.");

        task.AddRequiredPart(partNumber, description, quantity, unitOfMeasure, actorId);
        return DomainResult.Ok();
    }

    public DomainResult IssuePart(
        Guid taskId,
        Guid partId,
        string issueSlipRef,
        decimal issuedQuantity,
        Guid issuedByUserId,
        Guid actorId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            return DomainResult.Failure($"Task {taskId} not found.");

        return task.IssuePartFromStores(partId, issueSlipRef, issuedQuantity, issuedByUserId, actorId);
    }

    public DomainResult AddRequiredTool(
        Guid taskId,
        string toolNumber,
        string description,
        Guid actorId,
        DateOnly? calibratedExpiry = null)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            return DomainResult.Failure($"Task {taskId} not found.");

        task.AddRequiredTool(toolNumber, description, actorId, calibratedExpiry);
        return DomainResult.Ok();
    }

    public DomainResult CheckOutTool(Guid taskId, Guid toolId, Guid userId, Guid actorId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            return DomainResult.Failure($"Task {taskId} not found.");

        return task.CheckOutTool(toolId, userId, actorId);
    }

    public DomainResult ReturnTool(Guid taskId, Guid toolId, Guid actorId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            return DomainResult.Failure($"Task {taskId} not found.");

        return task.ReturnTool(toolId, actorId);
    }
}
