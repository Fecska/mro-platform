namespace Mro.Domain.Aggregates.WorkOrder.Enums;

/// <summary>
/// Classifies the kind of maintenance activity a Work Order covers.
/// Used for regulatory reporting and resource planning.
/// </summary>
public enum WorkOrderType
{
    /// <summary>Daily/overnight line check per the operator's maintenance programme.</summary>
    LineCheck,

    /// <summary>ICAO/EASA "A" base check (typically every 400–600 FH).</summary>
    AMaintenance,

    /// <summary>Intermediate "B" check (operator-specific interval).</summary>
    BMaintenance,

    /// <summary>Heavy "C" check (typically every 18–24 months or per FH).</summary>
    CMaintenance,

    /// <summary>Structural "D" check (typically every 6–12 years).</summary>
    DCheck,

    /// <summary>Non-routine card raised from a finding during scheduled maintenance.</summary>
    NonRoutine,

    /// <summary>Work raised to implement an Airworthiness Directive.</summary>
    AirworthinessDirectiveCompliance,

    /// <summary>Work raised to implement a Service Bulletin.</summary>
    ServiceBulletinCompliance,

    /// <summary>Rectification of a logged defect.</summary>
    DefectRectification,

    /// <summary>Planned component removal and installation (e.g. engine shop visit).</summary>
    ComponentChange,
}
