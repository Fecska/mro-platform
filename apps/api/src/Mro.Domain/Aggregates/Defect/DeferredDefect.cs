using Mro.Domain.Aggregates.Defect.Enums;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Defect;

/// <summary>
/// Records a formal deferral of a defect under MEL / CDL authority.
///
/// Invariants:
///   - A defect may have at most one active deferral at a time
///     (enforced by the Defect aggregate).
///   - DeferredUntil is computed from ExpiryDate; it may not be extended beyond
///     the MEL category maximum without a new DeferredDefect record.
///   - Only an authorised person (typically OrgAdmin or CertifyingStaff role)
///     may approve a deferral.
/// </summary>
public sealed class DeferredDefect : AuditableEntity
{
    public Guid DefectId { get; private set; }

    /// <summary>UTC date/time from which the deferral timer starts (normally aircraft departure).</summary>
    public DateTimeOffset DeferredFrom { get; private set; }

    /// <summary>Latest date/time by which rectification must be completed.</summary>
    public DateTimeOffset DeferredUntil { get; private set; }

    /// <summary>User who authorised the deferral (must hold appropriate authority).</summary>
    public Guid ApprovedByUserId { get; private set; }

    /// <summary>User who signed the deferral in the aircraft tech log.</summary>
    public Guid SignedByUserId { get; private set; }

    /// <summary>Flight or maintenance log entry number for traceability.</summary>
    public string? LogReference { get; private set; }

    /// <summary>
    /// Station (airport) at which the deferral was raised.
    /// Cross-module FK — plain Guid, no navigation.
    /// </summary>
    public Guid? StationId { get; private set; }

    /// <summary>Whether this deferral is still active or has been superseded / revoked.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>UTC timestamp when this deferral was revoked, if applicable.</summary>
    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>User who revoked the deferral, if applicable.</summary>
    public Guid? RevokedByUserId { get; private set; }

    /// <summary>Reason the deferral was revoked (e.g. "Defect rectified", "Re-evaluated").</summary>
    public string? RevocationReason { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow > DeferredUntil;

    // Owned MEL / CDL reference — navigated via shadow property "_melReference" in EF config
    private MelReference? _melReference;
    public MelReference? MelReference => _melReference;

    // EF Core
    private DeferredDefect() { }

    internal static DeferredDefect Create(
        Guid defectId,
        DateTimeOffset deferredFrom,
        DateTimeOffset deferredUntil,
        Guid approvedByUserId,
        Guid signedByUserId,
        Guid organisationId,
        Guid actorId,
        string? logReference = null,
        Guid? stationId = null)
    {
        if (deferredUntil <= deferredFrom)
            throw new ArgumentException(
                "DeferredUntil must be later than DeferredFrom.", nameof(deferredUntil));

        return new DeferredDefect
        {
            DefectId = defectId,
            DeferredFrom = deferredFrom,
            DeferredUntil = deferredUntil,
            ApprovedByUserId = approvedByUserId,
            SignedByUserId = signedByUserId,
            LogReference = logReference?.Trim(),
            StationId = stationId,
            IsActive = true,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Attaches the MEL/CDL reference to this deferral record.</summary>
    internal void AttachMelReference(MelReference melRef) => _melReference = melRef;

    internal void Revoke(string reason, Guid revokedByUserId, Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        IsActive = false;
        RevokedAt = DateTimeOffset.UtcNow;
        RevokedByUserId = revokedByUserId;
        RevocationReason = reason.Trim();
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
