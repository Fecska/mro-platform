namespace Mro.Domain.Aggregates.Aircraft.Enums;

/// <summary>
/// Operational status of an aircraft.  All state machine transitions are
/// enforced by the Aircraft aggregate — direct property mutations are not permitted.
///
/// Valid transitions:
///   active          → grounded, in_maintenance, withdrawn, written_off
///   grounded        → active, in_maintenance, withdrawn
///   in_maintenance  → active, grounded, withdrawn
///   withdrawn       → active, written_off
///   written_off     → (terminal — no outgoing transitions)
///
/// Stored as string in the database for readability (not as integer).
/// </summary>
public enum AircraftStatus
{
    /// <summary>In service and airworthy.</summary>
    Active,

    /// <summary>In service but temporarily unfit for flight (AOG or MEL item).</summary>
    Grounded,

    /// <summary>Undergoing a scheduled or unscheduled maintenance check.</summary>
    InMaintenance,

    /// <summary>Temporarily withdrawn from the fleet (storage, dry lease return, etc.).</summary>
    Withdrawn,

    /// <summary>Permanently removed from the register. Terminal state.</summary>
    WrittenOff,
}
