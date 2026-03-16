namespace Mro.Domain.Aggregates.WorkOrder.Enums;

/// <summary>
/// Role a person plays within a specific Work Order.
/// Distinct from the user's system-level Role — a CertifyingStaff user
/// may be assigned as a Mechanic on a WO that others will sign off.
/// </summary>
public enum AssignmentRole
{
    /// <summary>Performs physical maintenance work.</summary>
    Mechanic,

    /// <summary>Performs independent post-maintenance inspection.</summary>
    Inspector,

    /// <summary>Issues Certificate of Release to Service (Part-145 145.A.50).</summary>
    CertifyingStaff,

    /// <summary>Holds overall technical responsibility for the work package.</summary>
    Supervisor,

    /// <summary>Provides engineering disposition / concession authority.</summary>
    Engineer,
}
