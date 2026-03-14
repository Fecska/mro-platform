using Mro.Domain.Aggregates.Organisation.Events;
using Mro.Domain.Common.Audit;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Organisation;

/// <summary>
/// Aggregate root representing an MRO organisation holding a Part-145 approval.
/// All data in the system is scoped to an Organisation for multi-tenant isolation.
///
/// Invariants enforced here:
///   - Part-145 certificate number is immutable after creation
///   - An organisation must have at least one active station to process work orders
///   - An inactive organisation cannot have new work orders opened (checked at Application layer)
/// </summary>
public sealed class Organisation : AuditableEntity
{
    /// <summary>Legal trading name of the organisation.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// EASA Part-145 approval certificate reference number.
    /// Format varies by competent authority (e.g. "HU.145.0123", "UK.145.00456").
    /// Immutable after creation — changes require a new organisation record.
    /// </summary>
    public string Part145CertNumber { get; private set; } = string.Empty;

    /// <summary>
    /// ICAO organisation designator (if applicable) or internal short code.
    /// Used in document numbering (e.g. work order prefix "WO-{OrgCode}-{Year}-{Seq}").
    /// </summary>
    public string OrgCode { get; private set; } = string.Empty;

    /// <summary>Country code (ISO 3166-1 alpha-2) of the competent authority.</summary>
    public string CountryCode { get; private set; } = string.Empty;

    /// <summary>Primary contact email for the Accountable Manager.</summary>
    public string AccountableManagerEmail { get; private set; } = string.Empty;

    /// <summary>Whether this organisation is active on the platform.</summary>
    public bool IsActive { get; private set; } = true;

    private readonly List<Station> _stations = [];

    /// <summary>Stations belonging to this organisation.</summary>
    public IReadOnlyCollection<Station> Stations => _stations.AsReadOnly();

    // EF Core constructor
    private Organisation() { }

    /// <summary>
    /// Creates a new organisation and raises the <see cref="OrganisationCreatedEvent"/>.
    /// </summary>
    public static Organisation Create(
        string name,
        string part145CertNumber,
        string orgCode,
        string countryCode,
        string accountableManagerEmail,
        Guid createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(part145CertNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(orgCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountableManagerEmail);

        var org = new Organisation
        {
            Name = name.Trim(),
            Part145CertNumber = part145CertNumber.Trim(),
            OrgCode = orgCode.Trim().ToUpperInvariant(),
            CountryCode = countryCode.Trim().ToUpperInvariant(),
            AccountableManagerEmail = accountableManagerEmail.Trim().ToLowerInvariant(),
            OrganisationId = Guid.Empty, // self-referencing; set by persistence layer
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        org.RaiseDomainEvent(new OrganisationCreatedEvent
        {
            ActorId = createdBy,
            OrganisationId = org.Id,
            EntityType = nameof(Organisation),
            EntityId = org.Id,
            EventType = ComplianceEventType.RecordCreated,
            OrganisationName = org.Name,
            Part145CertNumber = org.Part145CertNumber,
            Description = $"Organisation '{org.Name}' (cert: {org.Part145CertNumber}) registered.",
        });

        return org;
    }

    /// <summary>
    /// Adds a new station to this organisation.
    /// </summary>
    public Station AddStation(
        string name,
        string icaoCode,
        string countryCode,
        Guid actorId)
    {
        var station = Station.Create(name, icaoCode, countryCode, Id, actorId);
        _stations.Add(station);

        RaiseDomainEvent(new StationAddedEvent
        {
            ActorId = actorId,
            OrganisationId = Id,
            EntityType = nameof(Station),
            EntityId = station.Id,
            EventType = ComplianceEventType.RecordCreated,
            StationId = station.Id,
            StationName = station.Name,
            IcaoCode = station.IcaoCode,
            Description = $"Station '{station.Name}' ({station.IcaoCode}) added to organisation '{Name}'.",
        });

        return station;
    }

    /// <summary>Deactivates the organisation. Existing records are not deleted.</summary>
    public void Deactivate(Guid actorId)
    {
        IsActive = false;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
