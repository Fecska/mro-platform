namespace Mro.Domain.Aggregates.WorkOrder.Enums;

/// <summary>
/// Lifecycle state of a Work Order.
/// Enum values intentionally match the frontend's snake_case labels (lowercased).
///
/// State machine:
///   Draft → Planned → Issued → InProgress
///   InProgress ⇌ WaitingParts | WaitingTooling
///   InProgress → WaitingInspection
///   WaitingInspection ⇌ InProgress (fail inspection → rework)
///   WaitingInspection → WaitingCertification
///   WaitingCertification → Completed
///   Completed → Closed
///   Any non-terminal except Completed/Closed → Cancelled
/// </summary>
public enum WorkOrderStatus
{
    /// <summary>WO created but not yet scheduled or resources planned.</summary>
    Draft,

    /// <summary>Scheduled; resources provisionally allocated.</summary>
    Planned,

    /// <summary>Released to hangar/line; work pack issued to technicians.</summary>
    Issued,

    /// <summary>Physical work actively in progress.</summary>
    InProgress,

    /// <summary>Work paused — waiting for required parts to arrive from stores.</summary>
    WaitingParts,

    /// <summary>Work paused — waiting for calibrated tooling.</summary>
    WaitingTooling,

    /// <summary>All task work complete; awaiting independent post-maintenance inspection.</summary>
    WaitingInspection,

    /// <summary>
    /// Inspection passed; awaiting Certifying Staff CRS sign-off.
    /// Part-145 AMC 145.A.50 requirement.
    /// </summary>
    WaitingCertification,

    /// <summary>CRS issued; work order technically complete.</summary>
    Completed,

    /// <summary>Administrative close — all documentation filed and archived.</summary>
    Closed,

    /// <summary>Cancelled before completion. Terminal state.</summary>
    Cancelled,
}
