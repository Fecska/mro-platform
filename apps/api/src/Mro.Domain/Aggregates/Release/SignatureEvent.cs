using Mro.Domain.Aggregates.Release.Enums;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Release;

/// <summary>
/// Immutable record of a single signature applied to a release certificate.
/// The certification statement is snapshotted at the moment of signing
/// to provide a durable compliance audit trail.
/// </summary>
public sealed class SignatureEvent : AuditableEntity
{
    public Guid CertificateId { get; private set; }
    public Guid SignerUserId { get; private set; }
    public DateTimeOffset SignedAt { get; private set; }

    /// <summary>Part-66 licence number or authorisation reference used to sign.</summary>
    public string LicenceRef { get; private set; } = null!;

    public SignatureMethod Method { get; private set; }

    /// <summary>
    /// Verbatim certification statement text, snapshotted at time of signing.
    /// Example: "I certify that the work specified was carried out in accordance with
    /// EASA Part-145 and in respect to that work the aircraft/aircraft component is
    /// considered ready for release to service."
    /// </summary>
    public string StatementText { get; private set; } = null!;

    public string? IpAddress { get; private set; }

    private SignatureEvent() { }

    internal static SignatureEvent Create(
        Guid certificateId,
        Guid signerUserId,
        string licenceRef,
        SignatureMethod method,
        string statementText,
        Guid organisationId,
        Guid actorId,
        string? ipAddress = null) => new()
    {
        CertificateId  = certificateId,
        SignerUserId   = signerUserId,
        SignedAt       = DateTimeOffset.UtcNow,
        LicenceRef     = licenceRef,
        Method         = method,
        StatementText  = statementText,
        IpAddress      = ipAddress,
        OrganisationId = organisationId,
        CreatedAt      = DateTimeOffset.UtcNow,
        CreatedBy      = actorId,
    };
}
