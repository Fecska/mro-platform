using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Common.Audit;

namespace Mro.Application.Features.Documents.Queries;

/// <summary>
/// Returns a pre-signed download URL for a specific revision.
/// Access is logged (DocumentAccessed) for Part-145 audit trail.
/// </summary>
public sealed record GetDownloadUrlQuery(Guid DocumentId, Guid RevisionId) : IRequest<Result<string>>;

public sealed class GetDownloadUrlQueryHandler(
    IDocumentRepository documents,
    IDocumentStorageService storage,
    IAuditService audit,
    ICurrentUserService currentUser)
    : IRequestHandler<GetDownloadUrlQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetDownloadUrlQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<string>(Error.Forbidden("Organisation context is required."));

        var doc = await documents.GetByIdAsync(request.DocumentId, currentUser.OrganisationId.Value, ct);
        if (doc is null)
            return Result.Failure<string>(
                Error.NotFound(nameof(Domain.Aggregates.Document.MaintenanceDocument), request.DocumentId));

        var revision = doc.Revisions.FirstOrDefault(r => r.Id == request.RevisionId);
        if (revision is null)
            return Result.Failure<string>(Error.NotFound("DocumentRevision", request.RevisionId));

        var url = await storage.GetDownloadUrlAsync(revision.StoragePath, expirySeconds: 300, ct);

        // Compliance requirement: every document access must be logged (REG-DOC-003)
        await audit.RecordComplianceEventAsync(
            ComplianceEventType.DocumentAccessed,
            entityType: nameof(Domain.Aggregates.Document.MaintenanceDocument),
            entityId: request.DocumentId,
            context: new { request.RevisionId, revision.RevisionNumber, doc.DocumentNumber, currentUser.IpAddress });

        return Result.Success(url);
    }
}
