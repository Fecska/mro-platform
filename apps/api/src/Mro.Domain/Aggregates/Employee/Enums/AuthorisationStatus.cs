namespace Mro.Domain.Aggregates.Employee.Enums;

public enum AuthorisationStatus
{
    /// <summary>Authorisation is active and within its validity period.</summary>
    Active,

    /// <summary>Authorisation has passed its ValidUntil date but has not been explicitly revoked.</summary>
    Expired,

    /// <summary>
    /// Authorisation is temporarily suspended (e.g. pending investigation).
    /// Can be reinstated. Suspended authorisations are not usable in workflows.
    /// </summary>
    Suspended,

    /// <summary>Authorisation was explicitly and permanently revoked.</summary>
    Revoked,
}
