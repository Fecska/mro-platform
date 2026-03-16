using Mro.Domain.Aggregates.Release.Enums;
using Mro.Domain.Aggregates.Release.Events;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Release;

/// <summary>
/// Certificate of Release to Service (CRS) or EASA Form 1.
///
/// Invariants:
///   - Only a Draft certificate may be submitted for signature.
///   - Only a PendingSignature certificate may be signed.
///   - A Signed certificate may only be voided by an AMO manager with documented reason.
///   - Void certificates cannot transition to any other state.
///   - Each Sign() call appends an immutable SignatureEvent.
///
/// State machine:
///   Draft → PendingSignature → Signed
///   Draft | PendingSignature | Signed → Void
/// </summary>
public sealed class ReleaseCertificate : AuditableEntity
{
    private readonly List<SignatureEvent> _signatures = [];

    /// <summary>System-generated number (e.g. "CRS-2025-00042" or "FORM1-2025-00001").</summary>
    public string CertificateNumber { get; private set; } = string.Empty;

    public CertificateType CertificateType { get; private set; }
    public CertificateStatus Status { get; private set; } = CertificateStatus.Draft;

    public Guid WorkOrderId { get; private set; }
    public Guid AircraftId { get; private set; }

    /// <summary>Snapshotted aircraft registration at time of certificate creation.</summary>
    public string AircraftRegistration { get; private set; } = null!;

    /// <summary>Snapshotted work order number.</summary>
    public string WorkOrderNumber { get; private set; } = null!;

    /// <summary>Human-readable description of work scope covered by this certificate.</summary>
    public string Scope { get; private set; } = null!;

    /// <summary>Regulatory basis (e.g. "EASA Part-145, AMM Chapter 5-20, AD 2024-001").</summary>
    public string RegulatoryBasis { get; private set; } = null!;

    public string? LimitationsAndRemarks { get; private set; }

    /// <summary>User designated as certifying staff for this certificate.</summary>
    public Guid CertifyingStaffUserId { get; private set; }

    public DateTimeOffset? IssuedAt { get; private set; }

    public string? VoidReason { get; private set; }

    public IReadOnlyList<SignatureEvent> Signatures => _signatures.AsReadOnly();

    private ReleaseCertificate() { }

    public static ReleaseCertificate Create(
        string certificateNumber,
        CertificateType certificateType,
        Guid workOrderId,
        Guid aircraftId,
        string aircraftRegistration,
        string workOrderNumber,
        string scope,
        string regulatoryBasis,
        Guid certifyingStaffUserId,
        Guid organisationId,
        Guid actorId,
        string? limitationsAndRemarks = null) => new()
    {
        CertificateNumber     = certificateNumber,
        CertificateType       = certificateType,
        WorkOrderId           = workOrderId,
        AircraftId            = aircraftId,
        AircraftRegistration  = aircraftRegistration,
        WorkOrderNumber       = workOrderNumber,
        Scope                 = scope,
        RegulatoryBasis       = regulatoryBasis,
        CertifyingStaffUserId = certifyingStaffUserId,
        LimitationsAndRemarks = limitationsAndRemarks,
        OrganisationId        = organisationId,
        CreatedAt             = DateTimeOffset.UtcNow,
        CreatedBy             = actorId,
    };

    // ── Submit for signature ───────────────────────────────────────────────

    public DomainResult Submit(Guid actorId)
    {
        if (Status != CertificateStatus.Draft)
            return DomainResult.Failure($"Only Draft certificates can be submitted (current: {Status}).");

        Status    = CertificateStatus.PendingSignature;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        return DomainResult.Ok();
    }

    // ── Sign ───────────────────────────────────────────────────────────────

    public DomainResult Sign(
        Guid signerUserId,
        string licenceRef,
        SignatureMethod method,
        string statementText,
        Guid actorId,
        string? ipAddress = null)
    {
        if (Status != CertificateStatus.PendingSignature)
            return DomainResult.Failure(
                $"Only PendingSignature certificates can be signed (current: {Status}).");
        if (string.IsNullOrWhiteSpace(licenceRef))
            return DomainResult.Failure("Licence reference is required for signing.");

        var signature = SignatureEvent.Create(
            Id, signerUserId, licenceRef, method, statementText, OrganisationId, actorId, ipAddress);
        _signatures.Add(signature);

        Status   = CertificateStatus.Signed;
        IssuedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;

        RaiseDomainEvent(new CertificateIssuedEvent
        {
            ActorId           = actorId,
            OrganisationId    = OrganisationId,
            EntityType        = "ReleaseCertificate",
            EntityId          = Id,
            EventType         = "CERTIFICATE_ISSUED",
            Description       = $"{CertificateType} {CertificateNumber} signed and issued by user {signerUserId}.",
            CertificateNumber = CertificateNumber,
            CertificateType   = CertificateType,
            WorkOrderId       = WorkOrderId,
            SignerUserId      = signerUserId,
        });

        return DomainResult.Ok();
    }

    // ── Void ───────────────────────────────────────────────────────────────

    public DomainResult Void(string reason, Guid actorId)
    {
        if (Status == CertificateStatus.Void)
            return DomainResult.Failure("Certificate is already void.");
        if (string.IsNullOrWhiteSpace(reason))
            return DomainResult.Failure("A void reason is required.");

        Status     = CertificateStatus.Void;
        VoidReason = reason;
        UpdatedAt  = DateTimeOffset.UtcNow;
        UpdatedBy  = actorId;

        RaiseDomainEvent(new CertificateVoidedEvent
        {
            ActorId           = actorId,
            OrganisationId    = OrganisationId,
            EntityType        = "ReleaseCertificate",
            EntityId          = Id,
            EventType         = "CERTIFICATE_VOIDED",
            Description       = $"{CertificateType} {CertificateNumber} voided. Reason: {reason}",
            CertificateNumber = CertificateNumber,
            VoidReason        = reason,
        });

        return DomainResult.Ok();
    }
}
