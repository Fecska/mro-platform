using Mro.Domain.Aggregates.Employee.Enums;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Employee;

/// <summary>
/// An aviation maintenance licence held by an employee.
/// Represents EASA Part-66, FAA A&amp;P, GCAA, or equivalent national licences.
///
/// Invariants:
///   - LicenceNumber must be unique within the issuing authority.
///   - ExpiresAt (medical validity date) when present must be in the future to remain current.
///   - Aircraft type ratings are stored as a comma-separated list for simplicity;
///     each rating is a short ICAO type designator (e.g. "B737", "A320").
/// </summary>
public sealed class Licence : AuditableEntity
{
    public Guid EmployeeId { get; private set; }

    public string LicenceNumber { get; private set; } = string.Empty;

    public LicenceCategory Category { get; private set; }

    /// <summary>
    /// Sub-category refinement (e.g. "B1.1", "B1.3", "B2L").
    /// Null for categories that have no sub-divisions (A, B3, C).
    /// </summary>
    public string? Subcategory { get; private set; }

    /// <summary>Issuing NAA or authority (e.g. "EASA", "UK CAA", "FAA", "GCAA").</summary>
    public string IssuingAuthority { get; private set; } = string.Empty;

    public DateOnly IssuedAt { get; private set; }

    /// <summary>
    /// Medical / revalidation expiry.
    /// Null for licences with no mandatory revalidation (e.g. basic Part-66 before type rating addition).
    /// </summary>
    public DateOnly? ExpiresAt { get; private set; }

    /// <summary>
    /// Comma-separated ICAO type designators for which type ratings are recorded
    /// (e.g. "B737,A320,B777"). Empty = basic licence, no type ratings yet.
    /// </summary>
    public string TypeRatings { get; private set; } = string.Empty;

    /// <summary>
    /// Free-text notes describing the privileges and limitations of this licence
    /// (e.g. "B1.1 – B737 Classic / NG only, no composite structures").
    /// </summary>
    public string? ScopeNotes { get; private set; }

    /// <summary>
    /// Storage reference to the scanned licence document (e.g. a Supabase / S3 object key or signed-URL path).
    /// </summary>
    public string? AttachmentRef { get; private set; }

    public bool IsExpired =>
        ExpiresAt.HasValue && ExpiresAt.Value < DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public bool IsCurrent => !IsExpired;

    // EF Core
    private Licence() { }

    internal static Licence Create(
        Guid employeeId,
        string licenceNumber,
        LicenceCategory category,
        string issuingAuthority,
        DateOnly issuedAt,
        Guid organisationId,
        Guid actorId,
        string? subcategory = null,
        DateOnly? expiresAt = null,
        string? typeRatings = null,
        string? scopeNotes = null,
        string? attachmentRef = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(licenceNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(issuingAuthority);

        return new Licence
        {
            EmployeeId = employeeId,
            LicenceNumber = licenceNumber.Trim().ToUpperInvariant(),
            Category = category,
            Subcategory = subcategory?.Trim(),
            IssuingAuthority = issuingAuthority.Trim(),
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt,
            TypeRatings = typeRatings?.Trim() ?? string.Empty,
            ScopeNotes = scopeNotes?.Trim(),
            AttachmentRef = attachmentRef?.Trim(),
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>Adds or updates an aircraft type rating on this licence.</summary>
    internal void AddTypeRating(string icaoTypeCode, Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(icaoTypeCode);

        var code = icaoTypeCode.Trim().ToUpperInvariant();
        var current = TypeRatings.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(r => r.Trim())
                                  .ToHashSet();

        if (!current.Contains(code))
        {
            current.Add(code);
            TypeRatings = string.Join(",", current.OrderBy(r => r));
            UpdatedBy = actorId;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>Updates the expiry date (e.g. after medical revalidation).</summary>
    internal void Revalidate(DateOnly newExpiresAt, Guid actorId)
    {
        ExpiresAt = newExpiresAt;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates mutable fields of the licence (expiry, scope notes, attachment).
    /// Called by the PATCH endpoint. Access modifier is internal so only the Employee aggregate
    /// or infrastructure can invoke it (accessed via ILicenceRepository for direct DB update).
    /// </summary>
    public DomainResult Update(
        DateOnly? expiresAt,
        bool clearExpiry,
        string? scopeNotes,
        string? attachmentRef,
        Guid actorId)
    {
        if (clearExpiry)
            ExpiresAt = null;
        else if (expiresAt.HasValue)
            ExpiresAt = expiresAt;

        if (scopeNotes is not null) ScopeNotes = scopeNotes.Trim();
        if (attachmentRef is not null) AttachmentRef = attachmentRef.Trim();

        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }
}
