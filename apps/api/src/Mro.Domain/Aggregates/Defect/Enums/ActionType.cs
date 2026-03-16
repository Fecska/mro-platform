namespace Mro.Domain.Aggregates.Defect.Enums;

/// <summary>
/// Classifies the kind of maintenance action recorded against a defect.
/// </summary>
public enum ActionType
{
    /// <summary>Physical repair or replacement work performed.</summary>
    Rectification,

    /// <summary>
    /// Independent inspection of the completed rectification
    /// (required before Cleared status; Part-145 AMC 145.A.65).
    /// </summary>
    Inspection,

    /// <summary>Operational test or function check carried out after rectification.</summary>
    FunctionalCheck,

    /// <summary>
    /// Engineering investigation to determine root cause
    /// (may precede rectification or result in a no-fault-found finding).
    /// </summary>
    Investigation,

    /// <summary>Temporary fix applied while awaiting permanent rectification.</summary>
    TemporaryFix,

    /// <summary>Deferral reviewed and re-approved (extends the deferral period).</summary>
    DeferralExtension,

    /// <summary>Parts or materials confirmed to be on order; logged for traceability.</summary>
    PartsOrdered,

    /// <summary>Certifying Staff sign-off entry (Certificate of Release to Service).</summary>
    CrsSignOff,
}
