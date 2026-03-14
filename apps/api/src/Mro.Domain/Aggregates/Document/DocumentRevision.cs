using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Document;

/// <summary>
/// One published revision (issue) of a maintenance document.
/// Only a single revision is marked IsCurrent at any time — enforced by
/// the MaintenanceDocument aggregate during AddRevision().
///
/// Revisions are never deleted; superseded revisions are kept for audit.
/// </summary>
public sealed class DocumentRevision : AuditableEntity
{
    public Guid DocumentId { get; private set; }

    /// <summary>Revision identifier as printed on the document (e.g. "Rev. 12", "Issue 3", "Amdt 7").</summary>
    public string RevisionNumber { get; private set; } = string.Empty;

    /// <summary>Date the revision was published by the issuer.</summary>
    public DateOnly IssuedAt { get; private set; }

    /// <summary>Date from which this revision is mandatory / effective.</summary>
    public DateOnly EffectiveAt { get; private set; }

    /// <summary>
    /// Blob storage path relative to the bucket root.
    /// Example: "documents/{orgId}/{docId}/{revId}.pdf"
    /// Never store a pre-signed URL here — URLs are generated on demand.
    /// </summary>
    public string StoragePath { get; private set; } = string.Empty;

    /// <summary>File size in bytes (for UI display and integrity tracking).</summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>SHA-256 hex digest of the uploaded file, for integrity verification.</summary>
    public string Sha256Checksum { get; private set; } = string.Empty;

    /// <summary>Whether this is the latest / current revision of the document.</summary>
    public bool IsCurrent { get; private set; }

    public Guid UploadedByUserId { get; private set; }

    // EF Core
    private DocumentRevision() { }

    internal static DocumentRevision Create(
        Guid documentId,
        string revisionNumber,
        DateOnly issuedAt,
        DateOnly effectiveAt,
        string storagePath,
        long fileSizeBytes,
        string sha256Checksum,
        Guid uploadedByUserId,
        Guid organisationId,
        Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(revisionNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(sha256Checksum);

        return new DocumentRevision
        {
            DocumentId = documentId,
            RevisionNumber = revisionNumber.Trim(),
            IssuedAt = issuedAt,
            EffectiveAt = effectiveAt,
            StoragePath = storagePath,
            FileSizeBytes = fileSizeBytes,
            Sha256Checksum = sha256Checksum.ToLowerInvariant(),
            IsCurrent = true,   // newly added revision becomes current; aggregate demotes previous
            UploadedByUserId = uploadedByUserId,
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    internal void DemoteFromCurrent(Guid actorId)
    {
        IsCurrent = false;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
