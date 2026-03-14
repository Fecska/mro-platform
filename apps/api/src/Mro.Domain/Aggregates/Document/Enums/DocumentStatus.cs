namespace Mro.Domain.Aggregates.Document.Enums;

/// <summary>
/// Lifecycle status of a maintenance document.
///
/// Transitions:
///   Draft       → Active, Cancelled
///   Active      → Superseded, Cancelled
///   Superseded  → (terminal)
///   Cancelled   → (terminal)
/// </summary>
public enum DocumentStatus
{
    /// <summary>Uploaded but not yet approved for use in maintenance tasks.</summary>
    Draft,

    /// <summary>Approved and available for task planning and execution.</summary>
    Active,

    /// <summary>Replaced by a newer document. Existing task links are preserved for history.</summary>
    Superseded,

    /// <summary>Withdrawn — must not be referenced in new tasks.</summary>
    Cancelled,
}
