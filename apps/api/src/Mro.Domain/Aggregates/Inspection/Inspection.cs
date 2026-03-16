using Mro.Domain.Aggregates.Inspection.Enums;
using Mro.Domain.Aggregates.Inspection.Events;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Inspection;

/// <summary>
/// An inspection performed as part of a work order or against a specific task.
///
/// Invariants:
///   - Only a Pending inspection can be started.
///   - Only an InProgress inspection can have its outcome recorded.
///   - Waiving requires an explicit justification string.
///   - A Failed inspection raises a compliance audit event (findings must be addressed).
///
/// State machine:
///   Pending → InProgress → Passed | Failed
///   Pending | InProgress → Waived
/// </summary>
public sealed class Inspection : AuditableEntity
{
    /// <summary>System-generated number (e.g. "INS-2025-00042").</summary>
    public string InspectionNumber { get; private set; } = string.Empty;

    public Guid WorkOrderId { get; private set; }

    /// <summary>Task-level inspection; null for work-order-level.</summary>
    public Guid? WorkOrderTaskId { get; private set; }

    public Guid AircraftId { get; private set; }
    public InspectionType InspectionType { get; private set; }
    public InspectionStatus Status { get; private set; } = InspectionStatus.Pending;

    /// <summary>User assigned as inspector (must hold appropriate Part-66 or authorisation).</summary>
    public Guid InspectorUserId { get; private set; }

    public DateTimeOffset? ScheduledAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    /// <summary>Free-text findings/remarks recorded during inspection.</summary>
    public string? Findings { get; private set; }

    /// <summary>Outcome remarks — mandatory when passing or failing.</summary>
    public string? OutcomeRemarks { get; private set; }

    /// <summary>Reason for waiving this inspection — mandatory when waiving.</summary>
    public string? WaiverReason { get; private set; }

    private Inspection() { }

    public static Inspection Create(
        string inspectionNumber,
        Guid workOrderId,
        Guid aircraftId,
        InspectionType inspectionType,
        Guid inspectorUserId,
        Guid organisationId,
        Guid actorId,
        Guid? workOrderTaskId = null,
        DateTimeOffset? scheduledAt = null)
    {
        var inspection = new Inspection
        {
            InspectionNumber = inspectionNumber,
            WorkOrderId      = workOrderId,
            WorkOrderTaskId  = workOrderTaskId,
            AircraftId       = aircraftId,
            InspectionType   = inspectionType,
            InspectorUserId  = inspectorUserId,
            ScheduledAt      = scheduledAt,
            OrganisationId   = organisationId,
            CreatedAt        = DateTimeOffset.UtcNow,
            CreatedBy        = actorId,
        };

        inspection.RaiseDomainEvent(new InspectionCreatedEvent
        {
            ActorId          = actorId,
            OrganisationId   = organisationId,
            EntityType       = "Inspection",
            EntityId         = inspection.Id,
            EventType        = "INSPECTION_CREATED",
            Description      = $"Inspection {inspectionNumber} created for work order {workOrderId}.",
            InspectionNumber = inspectionNumber,
            WorkOrderId      = workOrderId,
            InspectionType   = inspectionType,
        });

        return inspection;
    }

    // ── Start ──────────────────────────────────────────────────────────────

    public DomainResult Start(Guid actorId)
    {
        if (Status != InspectionStatus.Pending)
            return DomainResult.Failure($"Only Pending inspections can be started (current: {Status}).");

        Status    = InspectionStatus.InProgress;
        StartedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        return DomainResult.Ok();
    }

    // ── Record outcome ─────────────────────────────────────────────────────

    public DomainResult RecordOutcome(bool passed, string remarks, Guid actorId, string? findings = null)
    {
        if (Status != InspectionStatus.InProgress)
            return DomainResult.Failure($"Only InProgress inspections can have outcomes recorded (current: {Status}).");
        if (string.IsNullOrWhiteSpace(remarks))
            return DomainResult.Failure("Outcome remarks are required.");

        Status         = passed ? InspectionStatus.Passed : InspectionStatus.Failed;
        OutcomeRemarks = remarks;
        Findings       = findings;
        CompletedAt    = DateTimeOffset.UtcNow;
        UpdatedAt      = DateTimeOffset.UtcNow;
        UpdatedBy      = actorId;

        RaiseDomainEvent(new InspectionOutcomeRecordedEvent
        {
            ActorId          = actorId,
            OrganisationId   = OrganisationId,
            EntityType       = "Inspection",
            EntityId         = Id,
            EventType        = passed ? "INSPECTION_PASSED" : "INSPECTION_FAILED",
            Description      = $"Inspection {InspectionNumber} {Status.ToString().ToLower()}. {remarks}",
            InspectionNumber = InspectionNumber,
            Outcome          = Status,
        });

        return DomainResult.Ok();
    }

    // ── Waive ──────────────────────────────────────────────────────────────

    public DomainResult Waive(string reason, Guid actorId)
    {
        if (Status is InspectionStatus.Passed or InspectionStatus.Failed or InspectionStatus.Waived)
            return DomainResult.Failure($"Inspection is already in a terminal state ({Status}).");
        if (string.IsNullOrWhiteSpace(reason))
            return DomainResult.Failure("A waiver reason is required.");

        Status       = InspectionStatus.Waived;
        WaiverReason = reason;
        CompletedAt  = DateTimeOffset.UtcNow;
        UpdatedAt    = DateTimeOffset.UtcNow;
        UpdatedBy    = actorId;

        RaiseDomainEvent(new InspectionOutcomeRecordedEvent
        {
            ActorId          = actorId,
            OrganisationId   = OrganisationId,
            EntityType       = "Inspection",
            EntityId         = Id,
            EventType        = "INSPECTION_WAIVED",
            Description      = $"Inspection {InspectionNumber} waived. Reason: {reason}",
            InspectionNumber = InspectionNumber,
            Outcome          = InspectionStatus.Waived,
        });

        return DomainResult.Ok();
    }
}
