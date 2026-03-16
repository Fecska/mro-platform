namespace Mro.Domain.Aggregates.Employee.Enums;

public enum EmployeeStatus
{
    /// <summary>Active — may be assigned to work orders.</summary>
    Active,

    /// <summary>Temporarily absent (annual leave, sick leave, training).</summary>
    OnLeave,

    /// <summary>Suspended pending investigation; may not perform certifying actions.</summary>
    Suspended,

    /// <summary>Employment ended. Terminal state.</summary>
    Terminated,
}
