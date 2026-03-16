namespace Mro.Domain.Aggregates.Employee.Enums;

/// <summary>
/// Classifies a training record for compliance tracking and recurrency management.
/// </summary>
public enum TrainingType
{
    /// <summary>Initial / one-time qualification course (e.g. Part-66 initial, type training).</summary>
    Initial,

    /// <summary>Periodic refresher required to maintain currency (e.g. annual HF, SMS, SEP).</summary>
    Recurrent,

    /// <summary>Refresher triggered by an incident, audit finding, or period of inactivity.</summary>
    Refresher,

    /// <summary>Emergency procedures training (e.g. fire, first aid, emergency evacuation).</summary>
    Emergency,

    /// <summary>Simulator or practical task training session.</summary>
    Simulator,
}
