using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Documents.Dtos;

namespace Mro.Application.Features.Documents.Queries;

public sealed record GetRevisionsQuery(Guid DocumentId) : IRequest<Result<IReadOnlyList<RevisionDto>>>;

public sealed class GetRevisionsQueryHandler(
    IDocumentRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetRevisionsQuery, Result<IReadOnlyList<RevisionDto>>>
{
    public async Task<Result<IReadOnlyList<RevisionDto>>> Handle(
        GetRevisionsQuery request,
        CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<RevisionDto>>(Error.Forbidden("Organisation context is required."));

        var doc = await repository.GetByIdAsync(request.DocumentId, currentUser.OrganisationId.Value, ct);
        if (doc is null)
            return Result.Failure<IReadOnlyList<RevisionDto>>(
                Error.NotFound(nameof(Domain.Aggregates.Document.MaintenanceDocument), request.DocumentId));

        var dtos = doc.Revisions
            .OrderByDescending(r => r.EffectiveAt)
            .Select(r => new RevisionDto(
                r.Id,
                r.RevisionNumber,
                r.IssuedAt,
                r.EffectiveAt,
                r.FileSizeBytes,
                r.Sha256Checksum,
                r.IsCurrent,
                r.UploadedByUserId,
                r.CreatedAt))
            .ToList();

        return Result.Success<IReadOnlyList<RevisionDto>>(dtos);
    }
}
