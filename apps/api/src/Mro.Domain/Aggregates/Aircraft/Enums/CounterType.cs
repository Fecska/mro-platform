namespace Mro.Domain.Aggregates.Aircraft.Enums;

/// <summary>
/// Identifies the type of a life/utilisation counter on an aircraft.
/// Multiple counters of different types can exist on a single aircraft.
/// Stored as string in the database.
/// </summary>
public enum CounterType
{
    /// <summary>Total airframe flight hours (FAH / TTAF).</summary>
    TotalFlightHours,

    /// <summary>Total pressurisation cycles (FC).</summary>
    TotalFlightCycles,

    /// <summary>Total accumulated landings.</summary>
    TotalLandings,

    /// <summary>Engine #1 total time since new (TSN) in hours.</summary>
    Engine1Hours,

    /// <summary>Engine #1 cycles since new (CSN).</summary>
    Engine1Cycles,

    /// <summary>Engine #2 total time since new (TSN) in hours.</summary>
    Engine2Hours,

    /// <summary>Engine #2 cycles since new (CSN).</summary>
    Engine2Cycles,

    /// <summary>APU total time since new in hours (where applicable).</summary>
    ApuHours,

    /// <summary>APU cycles since new.</summary>
    ApuCycles,
}
