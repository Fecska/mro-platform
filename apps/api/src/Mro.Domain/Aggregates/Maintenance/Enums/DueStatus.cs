namespace Mro.Domain.Aggregates.Maintenance.Enums;

public enum DueStatus
{
    /// <summary>Next due date/hours is more than the threshold away.</summary>
    Current,

    /// <summary>Within the warning threshold (e.g. 30 days or 50 flight hours).</summary>
    DueSoon,

    /// <summary>Past the due date or exceeds the due hours/cycles.</summary>
    Overdue,

    /// <summary>Accomplished; no further action required until next interval.</summary>
    Accomplished,

    /// <summary>Not applicable to this aircraft variant/configuration.</summary>
    NotApplicable,
}
