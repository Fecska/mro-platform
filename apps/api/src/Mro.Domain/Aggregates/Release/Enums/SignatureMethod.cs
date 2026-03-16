namespace Mro.Domain.Aggregates.Release.Enums;

public enum SignatureMethod
{
    /// <summary>Qualified Electronic Signature (QES) via digital certificate.</summary>
    Digital,

    /// <summary>Simple electronic signature (username + password confirmation).</summary>
    Electronic,

    /// <summary>Wet-ink signature; scanned document attached.</summary>
    WetInk,
}
