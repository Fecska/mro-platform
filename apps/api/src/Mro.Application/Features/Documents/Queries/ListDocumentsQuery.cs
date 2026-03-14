using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Documents.Dtos;
using Mro.Domain.Aggregates.Document.Enums;

namespace Mro.Application.Features.Documents.Queries;

public sealed record ListDocumentsQuery(
    DocumentType? Type = null,
    DocumentStatus? Status = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<ListDocumentsResult>>;

public sealed record ListDocumentsResult(
    IReadOnlyList<DocumentSummaryDto> Items,
    int Total,
    int Page,
    int PageSize);

public sealed class ListDocumentsQueryHandler(
    IDocumentRepository documents,
    ICurrentUserService currentUser)
    : IRequestHandler<ListDocumentsQuery, Result<ListDocumentsResult>>
{
    public async Task<Result<ListDocumentsResult>> Handle(ListDocumentsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<ListDocumentsResult>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;
        var items = await documents.ListAsync(orgId, request.Type, request.Status, request.Page, request.PageSize, ct);
        var total = await documents.CountAsync(orgId, request.Type, request.Status, ct);

        var dtos = items.Select(doc =>
        {
            var cur = doc.CurrentRevision;
            return new DocumentSummaryDto(
                doc.Id,
                doc.DocumentNumber,
                doc.DocumentType.ToString(),
                doc.Title,
                doc.Issuer,
                doc.Status.ToString(),
                cur?.RevisionNumber,
                cur?.EffectiveAt);
        }).ToList();

        return Result.Success(new ListDocumentsResult(dtos, total, request.Page, request.PageSize));
    }
}
