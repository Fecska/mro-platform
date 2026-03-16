namespace Mro.Domain.Aggregates.Defect.Enums;

/// <summary>
/// Indicates how or where the defect was discovered.
/// Used for quality KPI reporting and root-cause analysis trending.
/// </summary>
public enum DefectSource
{
    /// <summary>Found during pre-flight or post-flight inspection by crew or line engineer.</summary>
    PilotReport,

    /// <summary>Discovered during scheduled maintenance inspection.</summary>
    RoutineMaintenance,

    /// <summary>Found during a non-scheduled maintenance visit.</summary>
    NonRoutineMaintenance,

    /// <summary>Identified during engineering or airworthiness review.</summary>
    EngineeringReview,

    /// <summary>Reported by cabin crew or passenger.</summary>
    CabinCrewReport,

    /// <summary>Transferred from a third-party organisation (e.g. handling agent, lessor).</summary>
    ThirdParty,

    /// <summary>Discovered during a Part-145 internal audit.</summary>
    InternalAudit,

    /// <summary>Identified by a regulatory authority (EASA, CAA, etc.) during oversight visit.</summary>
    RegulatoryAudit,
}
