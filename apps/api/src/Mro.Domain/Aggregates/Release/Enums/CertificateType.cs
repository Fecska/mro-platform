namespace Mro.Domain.Aggregates.Release.Enums;

public enum CertificateType
{
    /// <summary>Certificate of Release to Service (EASA Part-145 CRS).</summary>
    Crs,

    /// <summary>EASA Form 1 — authorised release certificate for components.</summary>
    Form1,

    /// <summary>Duplicate / replacement certificate issued after the original.</summary>
    Duplicate,
}
