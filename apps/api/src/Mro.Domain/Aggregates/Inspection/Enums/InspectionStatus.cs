namespace Mro.Domain.Aggregates.Inspection.Enums;

public enum InspectionStatus
{
    /// <summary>Inspection created but inspector not yet started.</summary>
    Pending,

    /// <summary>Inspector has started work.</summary>
    InProgress,

    /// <summary>Inspection completed with no findings (satisfactory).</summary>
    Passed,

    /// <summary>Inspection completed with findings; defects raised.</summary>
    Failed,

    /// <summary>Inspection waived by authorised engineer with documented justification.</summary>
    Waived,
}
