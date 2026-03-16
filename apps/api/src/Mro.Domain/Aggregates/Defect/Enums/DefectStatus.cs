namespace Mro.Domain.Aggregates.Defect.Enums;

/// <summary>
/// Lifecycle states of a maintenance defect.
///
/// State machine:
///   Reported → Triaged → Open → RectificationInProgress → InspectionPending → Cleared → Closed
///   Open → Deferred (MEL/CDL deferral recorded, timer running)
///   Deferred → Open (deferral expired or revoked), Deferred → RectificationInProgress
///   Any non-terminal state → Closed (with reason, by authorised staff)
///
/// Terminal states: Cleared, Closed.
/// </summary>
public enum DefectStatus
{
    /// <summary>Defect report submitted but not yet reviewed by an engineer.</summary>
    Reported,

    /// <summary>Engineer reviewed; priority and ownership assigned.</summary>
    Triaged,

    /// <summary>Accepted into the maintenance system; awaiting rectification.</summary>
    Open,

    /// <summary>Deferred under MEL/CDL authority; deferral timer is running.</summary>
    Deferred,

    /// <summary>Rectification work in progress.</summary>
    RectificationInProgress,

    /// <summary>Rectification done; awaiting independent inspection sign-off.</summary>
    InspectionPending,

    /// <summary>Independent inspection passed; defect rectification certified.</summary>
    Cleared,

    /// <summary>Defect closed without rectification (e.g. no fault found, design feature).</summary>
    Closed,
}
