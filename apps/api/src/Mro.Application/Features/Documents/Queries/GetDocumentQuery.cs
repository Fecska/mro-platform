using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Documents.Dtos;

namespace Mro.Application.Features.Documents.Queries;

public sealed record GetDocumentQuery(Guid DocumentId) : IRequest<Result<DocumentDetailDto>>;

public sealed class GetDocumentQueryHandler(
    IDocumentRepository documents,
    ICurrentUserService currentUser)
    : IRequestHandler<GetDocumentQuery, Result<DocumentDetailDto>>
{
    public async Task<Result<DocumentDetailDto>> Handle(GetDocumentQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<DocumentDetailDto>(Error.Forbidden("Organisation context is required."));

        var doc = await documents.GetByIdAsync(request.DocumentId, currentUser.OrganisationId.Value, ct);
        if (doc is null)
            return Result.Failure<DocumentDetailDto>(
                Error.NotFound(nameof(Domain.Aggregates.Document.MaintenanceDocument), request.DocumentId));

        return Result.Success(ToDto(doc));
    }

    internal static DocumentDetailDto ToDto(Domain.Aggregates.Document.MaintenanceDocument doc) =>
        new(
            doc.Id,
            doc.DocumentNumber,
            doc.DocumentType.ToString(),
            doc.Title,
            doc.Issuer,
            doc.Status.ToString(),
            doc.RegulatoryReference,
            doc.SupersedesDocumentId,
            doc.SupersededByDocumentId,
            doc.Revisions.OrderByDescending(r => r.EffectiveAt).Select(r => new RevisionDto(
                r.Id, r.RevisionNumber, r.IssuedAt, r.EffectiveAt,
                r.FileSizeBytes, r.Sha256Checksum, r.IsCurrent, r.UploadedByUserId, r.CreatedAt)).ToList(),
            doc.Effectivities.Select(e => new EffectivityDto(
                e.Id, e.IcaoTypeCode, e.SerialFrom, e.SerialTo, e.ConditionNote)).ToList()
        );
}
