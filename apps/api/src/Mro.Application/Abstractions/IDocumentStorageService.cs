namespace Mro.Application.Abstractions;

/// <summary>
/// Abstracts blob storage operations for maintenance documents.
/// Implemented in Mro.Infrastructure using Amazon S3 / MinIO.
///
/// Files are never served directly — only pre-signed URLs are returned.
/// Access to these URLs is time-limited (default 5 minutes) and logged.
/// </summary>
public interface IDocumentStorageService
{
    /// <summary>
    /// Generates a time-limited pre-signed download URL for the given storage path.
    /// The URL expires after <paramref name="expirySeconds"/> seconds (default: 300).
    /// </summary>
    Task<string> GetDownloadUrlAsync(
        string storagePath,
        int expirySeconds = 300,
        CancellationToken ct = default);

    /// <summary>
    /// Computes the canonical storage path for a new revision.
    /// Format: "documents/{orgId}/{docId}/{revId}.pdf"
    /// </summary>
    string BuildStoragePath(Guid organisationId, Guid documentId, Guid revisionId);
}
