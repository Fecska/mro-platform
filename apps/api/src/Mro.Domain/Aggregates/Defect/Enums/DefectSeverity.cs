namespace Mro.Domain.Aggregates.Defect.Enums;

/// <summary>
/// Severity classification of a maintenance defect.
/// Maps to engineering risk assessment and MEL category.
/// </summary>
public enum DefectSeverity
{
    /// <summary>Go/No-Go defect; aircraft cannot be dispatched until rectified.</summary>
    Critical,

    /// <summary>
    /// Significant airworthiness impact; deferral possible under MEL Cat A/B
    /// but requires management approval.
    /// </summary>
    High,

    /// <summary>Moderate impact; standard MEL Cat C/D deferral typically applicable.</summary>
    Medium,

    /// <summary>Cosmetic or non-airworthiness-affecting; long deferral or cosmetic backlog.</summary>
    Low,
}
