using Mro.Domain.Aggregates.Employee.Enums;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Employee;

/// <summary>
/// A planned or worked shift for an employee.
/// Used for resource availability planning and fatigue management tracking.
///
/// Invariants:
///   - EndTime must be after StartTime (no same-day midnight-crossing; use two shifts for split days).
///   - No two shifts for the same employee may overlap in time (enforced by the Employee aggregate).
/// </summary>
public sealed class Shift : AuditableEntity
{
    public Guid EmployeeId { get; private set; }

    public DateOnly ShiftDate { get; private set; }

    /// <summary>Local start time at the station (stored without timezone; tzinfo on StationId).</summary>
    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    /// <summary>
    /// Station where the shift is worked.
    /// Cross-module FK — plain Guid, no navigation.
    /// </summary>
    public Guid? StationId { get; private set; }

    public ShiftType ShiftType { get; private set; }

    /// <summary>
    /// Whether the employee is available for task assignment during this shift.
    /// Standby, Training, OnLeave etc. make the employee unavailable for work-order assignment.
    /// </summary>
    public AvailabilityStatus AvailabilityStatus { get; private set; }

    /// <summary>True = actual worked hours; false = planned/rostered shift.</summary>
    public bool IsActual { get; private set; }

    public string? Notes { get; private set; }

    /// <summary>Shift duration in decimal hours.</summary>
    public decimal Hours =>
        (decimal)(EndTime - StartTime).TotalHours;

    /// <summary>
    /// Returns the shift window as a comparable DateTime range for overlap detection.
    /// Uses a fixed epoch date so times-only are comparable across date boundaries.
    /// </summary>
    internal DateTime StartDateTime => ShiftDate.ToDateTime(StartTime);
    internal DateTime EndDateTime   => ShiftDate.ToDateTime(EndTime);

    /// <summary>
    /// True when this shift's time window overlaps with the given candidate window.
    /// Two windows overlap when: candidateStart &lt; thisEnd AND candidateEnd &gt; thisStart.
    /// </summary>
    internal bool OverlapsWith(DateTime candidateStart, DateTime candidateEnd) =>
        candidateStart < EndDateTime && candidateEnd > StartDateTime;

    // EF Core
    private Shift() { }

    internal static Shift Create(
        Guid employeeId,
        DateOnly shiftDate,
        TimeOnly startTime,
        TimeOnly endTime,
        ShiftType shiftType,
        AvailabilityStatus availabilityStatus,
        bool isActual,
        Guid organisationId,
        Guid actorId,
        Guid? stationId = null,
        string? notes = null)
    {
        if (endTime <= startTime)
            throw new ArgumentException("Shift end time must be after start time.", nameof(endTime));

        return new Shift
        {
            EmployeeId         = employeeId,
            ShiftDate          = shiftDate,
            StartTime          = startTime,
            EndTime            = endTime,
            ShiftType          = shiftType,
            AvailabilityStatus = availabilityStatus,
            IsActual           = isActual,
            StationId          = stationId,
            Notes              = notes?.Trim(),
            OrganisationId     = organisationId,
            CreatedBy          = actorId,
            UpdatedBy          = actorId,
            CreatedAt          = DateTimeOffset.UtcNow,
            UpdatedAt          = DateTimeOffset.UtcNow,
        };
    }
}
