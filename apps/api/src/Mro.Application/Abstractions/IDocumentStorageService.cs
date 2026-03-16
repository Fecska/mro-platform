namespace Mro.Application.Abstractions;

/// <summary>
/// Abstracts blob storage operations for maintenance documents and employee attachments.
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
    /// Generates a time-limited pre-signed URL for a direct PUT upload to blob storage.
    /// The caller uploads the file using an HTTP PUT to the returned URL.
    /// </summary>
    Task<string> GetUploadUrlAsync(
        string storagePath,
        string contentType,
        int expirySeconds = 300,
        CancellationToken ct = default);

    /// <summary>
    /// Computes the canonical storage path for a new document revision.
    /// Format: "documents/{orgId}/{docId}/{revId}.pdf"
    /// </summary>
    string BuildStoragePath(Guid organisationId, Guid documentId, Guid revisionId);

    /// <summary>
    /// Computes the canonical storage path for an employee attachment.
    /// Format: "employee-attachments/{orgId}/{employeeId}/{attachmentId}{extension}"
    /// </summary>
    string BuildEmployeeAttachmentPath(
        Guid organisationId,
        Guid employeeId,
        Guid attachmentId,
        string extension);
}
