using Mro.Domain.Aggregates.Employee.Enums;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Employee;

/// <summary>
/// An organisation-issued authorisation granting an employee the right
/// to certify maintenance work within a defined scope.
///
/// Invariants:
///   - Only one active authorisation per scope/category combination (HS-011).
///   - IssuingLicenceId must point to a current, non-expired licence
///     of equal or broader category (enforced by the Employee aggregate).
///   - Revoking an authorisation is permanent; a new one must be issued after re-qualification.
///   - Suspending is temporary and reversible; suspended authorisations are not usable in workflows.
///   - RevisionNumber increments with each state change; provides lightweight versioning.
/// </summary>
public sealed class Authorisation : AuditableEntity
{
    public Guid EmployeeId { get; private set; }

    public string AuthorisationNumber { get; private set; } = string.Empty;

    public LicenceCategory Category { get; private set; }

    /// <summary>
    /// Refined scope string (e.g. "B1.1", "B2L", "C", "A – B737").
    /// More specific than Category; used for display and planning matching.
    /// </summary>
    public string Scope { get; private set; } = string.Empty;

    /// <summary>
    /// Comma-separated ICAO type designators this authorisation covers.
    /// Empty = all types within the scope.
    /// </summary>
    public string AircraftTypes { get; private set; } = string.Empty;

    /// <summary>
    /// Specific component families or ATA chapter groups covered
    /// (e.g. "Landing Gear", "Avionics – ATA 23/34"). Null = unrestricted.
    /// </summary>
    public string? ComponentScope { get; private set; }

    /// <summary>
    /// Station or base restriction (e.g. "DXB only", "Line stations"). Null = all stations.
    /// </summary>
    public string? StationScope { get; private set; }

    /// <summary>
    /// The company or competent authority that issued the authorisation
    /// (e.g. organisation legal name, CAMO name, or NAA reference).
    /// </summary>
    public string IssuingAuthority { get; private set; } = string.Empty;

    public DateOnly ValidFrom { get; private set; }

    public DateOnly? ValidUntil { get; private set; }

    /// <summary>The underlying Part-66 (or equivalent) licence this authorisation is based on.</summary>
    public Guid IssuingLicenceId { get; private set; }

    public Guid IssuedByUserId { get; private set; }

    /// <summary>False only when permanently Revoked.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// True when temporarily suspended. Suspended authorisations are NOT usable in workflows
    /// (IsCurrent returns false) but can be reinstated via Resume().
    /// </summary>
    public bool IsSuspended { get; private set; }

    public string? SuspensionReason { get; private set; }

    public DateTimeOffset? SuspendedAt { get; private set; }

    public Guid? SuspendedByUserId { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public Guid? RevokedByUserId { get; private set; }

    public string? RevocationReason { get; private set; }

    /// <summary>
    /// Monotonically increasing version counter.
    /// Starts at 1 on creation; incremented by every state-changing method.
    /// Allows clients to detect concurrent modifications.
    /// </summary>
    public int RevisionNumber { get; private set; } = 1;

    public bool IsExpired =>
        ValidUntil.HasValue && ValidUntil.Value < DateOnly.FromDateTime(DateTime.UtcNow.Date);

    /// <summary>
    /// True only when Active (not revoked, not suspended) and not expired.
    /// This is the flag checked by workflows (HS-011, CheckPrerequisites).
    /// </summary>
    public bool IsCurrent => IsActive && !IsSuspended && !IsExpired;

    public AuthorisationStatus Status =>
        !IsActive   ? AuthorisationStatus.Revoked :
        IsSuspended ? AuthorisationStatus.Suspended :
        IsExpired   ? AuthorisationStatus.Expired :
                      AuthorisationStatus.Active;

    // EF Core
    private Authorisation() { }

    internal static Authorisation Create(
        Guid employeeId,
        string authorisationNumber,
        LicenceCategory category,
        string scope,
        Guid issuingLicenceId,
        Guid issuedByUserId,
        DateOnly validFrom,
        Guid organisationId,
        Guid actorId,
        string? aircraftTypes = null,
        DateOnly? validUntil = null,
        string? componentScope = null,
        string? stationScope = null,
        string? issuingAuthority = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorisationNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);

        return new Authorisation
        {
            EmployeeId          = employeeId,
            AuthorisationNumber = authorisationNumber.Trim().ToUpperInvariant(),
            Category            = category,
            Scope               = scope.Trim(),
            AircraftTypes       = aircraftTypes?.Trim() ?? string.Empty,
            ComponentScope      = componentScope?.Trim(),
            StationScope        = stationScope?.Trim(),
            IssuingAuthority    = (issuingAuthority ?? string.Empty).Trim(),
            ValidFrom           = validFrom,
            ValidUntil          = validUntil,
            IssuingLicenceId    = issuingLicenceId,
            IssuedByUserId      = issuedByUserId,
            IsActive            = true,
            IsSuspended         = false,
            RevisionNumber      = 1,
            OrganisationId      = organisationId,
            CreatedBy           = actorId,
            UpdatedBy           = actorId,
            CreatedAt           = DateTimeOffset.UtcNow,
            UpdatedAt           = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Updates mutable scope fields and/or extends validity.
    /// Increments RevisionNumber so callers can detect the change.
    /// </summary>
    public DomainResult Amend(
        DateOnly? validUntil,
        bool clearValidUntil,
        string? aircraftTypes,
        string? componentScope,
        string? stationScope,
        string? issuingAuthority,
        Guid actorId)
    {
        if (!IsActive)
            return DomainResult.Failure("Cannot amend a revoked authorisation.");

        if (clearValidUntil)
            ValidUntil = null;
        else if (validUntil.HasValue)
            ValidUntil = validUntil;

        if (aircraftTypes is not null)    AircraftTypes    = aircraftTypes.Trim();
        if (componentScope is not null)   ComponentScope   = componentScope.Trim();
        if (stationScope is not null)     StationScope     = stationScope.Trim();
        if (issuingAuthority is not null) IssuingAuthority = issuingAuthority.Trim();

        RevisionNumber++;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    /// <summary>
    /// Temporarily suspends the authorisation.
    /// The authorisation cannot be used in workflows while suspended (IsCurrent = false).
    /// </summary>
    public DomainResult Suspend(string reason, Guid suspendedByUserId, Guid actorId)
    {
        if (!IsActive)
            return DomainResult.Failure("Cannot suspend a revoked authorisation.");

        if (IsSuspended)
            return DomainResult.Failure("Authorisation is already suspended.");

        IsSuspended        = true;
        SuspensionReason   = reason.Trim();
        SuspendedAt        = DateTimeOffset.UtcNow;
        SuspendedByUserId  = suspendedByUserId;
        RevisionNumber++;
        UpdatedBy          = actorId;
        UpdatedAt          = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    /// <summary>Reinstates a previously suspended authorisation.</summary>
    public DomainResult Resume(Guid actorId)
    {
        if (!IsActive)
            return DomainResult.Failure("Cannot resume a revoked authorisation.");

        if (!IsSuspended)
            return DomainResult.Failure("Authorisation is not suspended.");

        IsSuspended       = false;
        SuspensionReason  = null;
        SuspendedAt       = null;
        SuspendedByUserId = null;
        RevisionNumber++;
        UpdatedBy         = actorId;
        UpdatedAt         = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    internal DomainResult Revoke(string reason, Guid revokedByUserId, Guid actorId)
    {
        if (!IsActive)
            return DomainResult.Failure("Authorisation is already revoked.");

        IsActive         = false;
        IsSuspended      = false;
        RevokedAt        = DateTimeOffset.UtcNow;
        RevokedByUserId  = revokedByUserId;
        RevocationReason = reason.Trim();
        RevisionNumber++;
        UpdatedBy        = actorId;
        UpdatedAt        = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }
}
