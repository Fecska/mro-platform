using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Mro.Application.Abstractions;

namespace Mro.Infrastructure.Storage;

/// <summary>
/// Blob storage service backed by Amazon S3 or MinIO (S3-compatible).
///
/// Configuration keys:
///   Storage:BucketName      — target bucket (default: "mro-documents")
///   Storage:ServiceUrl      — for MinIO: "http://localhost:9000"; omit for real AWS
///   AWS:AccessKey           — or use IAM role in production
///   AWS:SecretKey
///   AWS:Region              — default: "eu-central-1"
/// </summary>
public sealed class S3DocumentStorageService : IDocumentStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public S3DocumentStorageService(IAmazonS3 s3, IConfiguration configuration)
    {
        _s3 = s3;
        _bucket = configuration["Storage:BucketName"] ?? "mro-documents";
    }

    public async Task<string> GetDownloadUrlAsync(
        string storagePath,
        int expirySeconds = 300,
        CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = storagePath,
            Expires = DateTime.UtcNow.AddSeconds(expirySeconds),
            Verb = HttpVerb.GET,
        };

        // GetPreSignedURL is synchronous in AWSSDK.S3 v3
        return await Task.FromResult(_s3.GetPreSignedURL(request));
    }

    public string BuildStoragePath(Guid organisationId, Guid documentId, Guid revisionId) =>
        $"documents/{organisationId}/{documentId}/{revisionId}.pdf";
}
