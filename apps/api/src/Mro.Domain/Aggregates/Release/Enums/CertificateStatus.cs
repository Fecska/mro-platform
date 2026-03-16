namespace Mro.Domain.Aggregates.Release.Enums;

public enum CertificateStatus
{
    /// <summary>Certificate is being prepared; not yet submitted for signature.</summary>
    Draft,

    /// <summary>Submitted to certifying staff; awaiting signature.</summary>
    PendingSignature,

    /// <summary>
    /// Signed and released. Terminal state under normal operations.
    /// A signed CRS may only be voided by an AMO manager with documented reason.
    /// </summary>
    Signed,

    /// <summary>Certificate declared void; a new certificate must be raised if required.</summary>
    Void,
}
