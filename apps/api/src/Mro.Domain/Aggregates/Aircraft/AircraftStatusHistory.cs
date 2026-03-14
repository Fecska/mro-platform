using Mro.Domain.Aggregates.Aircraft.Enums;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Aircraft;

/// <summary>
/// Immutable record of a single aircraft status transition.
/// Appended by Aircraft.ChangeStatus() — never updated or deleted.
/// Provides a full audit trail of the aircraft's operational history.
/// </summary>
public sealed class AircraftStatusHistory : BaseEntity
{
    public Guid AircraftId { get; private set; }
    public AircraftStatus FromStatus { get; private set; }
    public AircraftStatus ToStatus { get; private set; }

    /// <summary>Mandatory reason for the status change (e.g. "AOG — hydraulic leak").</summary>
    public string Reason { get; private set; } = string.Empty;

    public Guid ActorId { get; private set; }
    public Guid OrganisationId { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }

    // EF Core
    private AircraftStatusHistory() { }

    internal static AircraftStatusHistory Create(
        Guid aircraftId,
        AircraftStatus from,
        AircraftStatus to,
        string reason,
        Guid actorId,
        Guid organisationId)
    {
        return new AircraftStatusHistory
        {
            AircraftId = aircraftId,
            FromStatus = from,
            ToStatus = to,
            Reason = reason,
            ActorId = actorId,
            OrganisationId = organisationId,
            ChangedAt = DateTimeOffset.UtcNow,
        };
    }
}
