using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Aircraft;

/// <summary>
/// A certified aircraft type (type certificate holder + model).
/// Types are organisation-scoped in the MVP; a shared global registry
/// can be introduced later via a separate module without domain changes.
///
/// Examples: Boeing 737-800 (ICAO: B738), Airbus A320-200 (ICAO: A320).
/// </summary>
public sealed class AircraftType : AuditableEntity
{
    /// <summary>ICAO aircraft type designator (4 chars, uppercase). e.g. "B738", "A320".</summary>
    public string IcaoTypeCode { get; private set; } = string.Empty;

    /// <summary>Type certificate holder / manufacturer. e.g. "Boeing", "Airbus".</summary>
    public string Manufacturer { get; private set; } = string.Empty;

    /// <summary>Model / series designation. e.g. "737-800", "A320-200".</summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>Maximum passenger seats (informational).</summary>
    public int? MaxSeats { get; private set; }

    public bool IsActive { get; private set; } = true;

    // EF Core
    private AircraftType() { }

    public static AircraftType Create(
        string icaoTypeCode,
        string manufacturer,
        string model,
        Guid organisationId,
        Guid createdBy,
        int? maxSeats = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(icaoTypeCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(manufacturer);
        ArgumentException.ThrowIfNullOrWhiteSpace(model);

        return new AircraftType
        {
            IcaoTypeCode = icaoTypeCode.Trim().ToUpperInvariant(),
            Manufacturer = manufacturer.Trim(),
            Model = model.Trim(),
            MaxSeats = maxSeats,
            OrganisationId = organisationId,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void Deactivate(Guid actorId)
    {
        IsActive = false;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
