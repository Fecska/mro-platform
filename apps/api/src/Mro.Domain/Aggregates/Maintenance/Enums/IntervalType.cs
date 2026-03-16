namespace Mro.Domain.Aggregates.Maintenance.Enums;

public enum IntervalType
{
    /// <summary>Recurring by calendar time (days).</summary>
    CalendarDays,

    /// <summary>Recurring by accumulated flight hours.</summary>
    FlightHours,

    /// <summary>Recurring by landing cycle count.</summary>
    Cycles,

    /// <summary>Recurring by pressurisation cycles.</summary>
    PressurisationCycles,

    /// <summary>Whichever of FlightHours or CalendarDays falls first.</summary>
    FlightHoursOrCalendarDays,

    /// <summary>One-time task; no recurrence.</summary>
    OneTime,
}
