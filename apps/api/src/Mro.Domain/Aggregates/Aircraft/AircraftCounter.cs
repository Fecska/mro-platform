using Mro.Domain.Aggregates.Aircraft.Enums;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Aircraft;

/// <summary>
/// One utilisation counter on an aircraft (e.g. total flight hours, engine cycles).
/// Values are monotonically increasing; decreasing a counter is a hard stop.
///
/// Owned by the Aircraft aggregate — update only through Aircraft.UpdateCounter().
/// </summary>
public sealed class AircraftCounter : AuditableEntity
{
    public Guid AircraftId { get; private set; }
    public CounterType CounterType { get; private set; }

    /// <summary>Current accumulated value (hours as decimal, cycles as integer-valued decimal).</summary>
    public decimal Value { get; private set; }

    public DateTimeOffset LastUpdatedAt { get; private set; }

    // EF Core
    private AircraftCounter() { }

    internal static AircraftCounter Create(
        Guid aircraftId,
        CounterType counterType,
        decimal initialValue,
        Guid organisationId,
        Guid actorId)
    {
        return new AircraftCounter
        {
            AircraftId = aircraftId,
            CounterType = counterType,
            Value = initialValue,
            LastUpdatedAt = DateTimeOffset.UtcNow,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <returns>False if the new value would decrease the counter (not permitted).</returns>
    internal bool TrySetValue(decimal newValue, Guid actorId)
    {
        if (newValue < Value) return false;

        Value = newValue;
        LastUpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }
}
