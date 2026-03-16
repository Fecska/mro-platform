namespace Mro.Domain.Aggregates.WorkOrder.Enums;

/// <summary>
/// Status of an individual task card within a Work Order.
/// </summary>
public enum WorkOrderTaskStatus
{
    /// <summary>Task is planned but work has not started.</summary>
    Pending,

    /// <summary>Technician has started work on this task.</summary>
    InProgress,

    /// <summary>Work complete; awaiting inspection or CRS sign-off.</summary>
    Completed,

    /// <summary>
    /// Independent inspection and CRS sign-off recorded.
    /// Terminal state for a task — cannot be un-signed without a revision.
    /// </summary>
    SignedOff,

    /// <summary>Task cancelled (e.g. no-fault-found, scope change).</summary>
    Cancelled,
}
