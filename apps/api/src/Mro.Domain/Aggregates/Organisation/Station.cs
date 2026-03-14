using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Organisation;

/// <summary>
/// Represents a physical maintenance station (base, hangar, or line station).
/// Stations are the primary unit for operational scoping — users are granted
/// access to one or more stations within their organisation.
///
/// Stations are owned by an Organisation and are not aggregates in their own right;
/// they are created through the Organisation aggregate to enforce invariants.
/// </summary>
public sealed class Station : AuditableEntity
{
    /// <summary>Display name of the station (e.g. "Budapest Line Station").</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// ICAO airport code where this station operates (e.g. "LHBP", "EGLL").
    /// Four letters, uppercase.  Not globally unique — an organisation may have
    /// multiple stations at the same airport.
    /// </summary>
    public string IcaoCode { get; private set; } = string.Empty;

    /// <summary>Country code (ISO 3166-1 alpha-2) for regulatory jurisdiction.</summary>
    public string CountryCode { get; private set; } = string.Empty;

    /// <summary>Whether this station is currently operational.</summary>
    public bool IsActive { get; private set; } = true;

    // EF Core constructor
    private Station() { }

    internal static Station Create(
        string name,
        string icaoCode,
        string countryCode,
        Guid organisationId,
        Guid createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(icaoCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        return new Station
        {
            Name = name.Trim(),
            IcaoCode = icaoCode.Trim().ToUpperInvariant(),
            CountryCode = countryCode.Trim().ToUpperInvariant(),
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
