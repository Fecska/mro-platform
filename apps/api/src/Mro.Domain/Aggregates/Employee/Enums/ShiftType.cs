namespace Mro.Domain.Aggregates.Employee.Enums;

public enum ShiftType
{
    /// <summary>Standard daytime shift (e.g. 06:00–14:00).</summary>
    Day,

    /// <summary>Evening or afternoon shift (e.g. 14:00–22:00).</summary>
    Evening,

    /// <summary>Overnight shift (e.g. 22:00–06:00).</summary>
    Night,

    /// <summary>Employee is on standby and contactable but not at the workplace.</summary>
    Standby,

    /// <summary>Employee is on-call and must respond within a defined response time.</summary>
    OnCall,

    /// <summary>Two separate duty periods within the same calendar day.</summary>
    Split,

    /// <summary>Administrative or office day (not line maintenance).</summary>
    Admin,
}
