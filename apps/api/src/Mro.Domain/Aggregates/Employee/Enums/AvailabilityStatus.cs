namespace Mro.Domain.Aggregates.Employee.Enums;

public enum AvailabilityStatus
{
    /// <summary>Employee is present and available for task assignment.</summary>
    Available,

    /// <summary>Employee is scheduled off duty and not available.</summary>
    OffDuty,

    /// <summary>Employee is on approved annual or personal leave.</summary>
    OnLeave,

    /// <summary>Employee is absent due to illness or medical reasons.</summary>
    SickLeave,

    /// <summary>Employee is attending training and unavailable for maintenance tasks.</summary>
    Training,

    /// <summary>Employee is on-call or standby but not at the station.</summary>
    Standby,

    /// <summary>Employee is temporarily unavailable due to an administrative reason (e.g. audit, meeting).</summary>
    Unavailable,
}
