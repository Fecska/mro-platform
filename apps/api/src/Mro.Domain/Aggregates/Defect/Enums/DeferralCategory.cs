namespace Mro.Domain.Aggregates.Defect.Enums;

/// <summary>
/// MEL / CDL dispatch category that governs the maximum allowable deferral period.
///
/// Per EASA Part-26 / MMEL policy:
///   Cat A — as specified in the MEL (may be days or flight cycles)
///   Cat B — 3 consecutive calendar days (72 hours)
///   Cat C — 10 consecutive calendar days
///   Cat D — 120 consecutive calendar days
///
/// CDL items have their own interval specified per-item in the CDL document.
/// </summary>
public enum DeferralCategory
{
    /// <summary>MEL Category A — operator-specific interval stated in the MEL.</summary>
    MelA,

    /// <summary>MEL Category B — 3 consecutive calendar days.</summary>
    MelB,

    /// <summary>MEL Category C — 10 consecutive calendar days.</summary>
    MelC,

    /// <summary>MEL Category D — 120 consecutive calendar days.</summary>
    MelD,

    /// <summary>Configuration Deviation List item — interval per CDL entry.</summary>
    Cdl,

    /// <summary>
    /// Deferred by engineering order with a custom interval
    /// (used when the MEL does not cover the specific failure mode).
    /// </summary>
    EngineeringOrder,
}
