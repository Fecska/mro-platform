using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Documents.Commands;

/// <summary>
/// Registers a new document revision.  The file must already be uploaded to blob
/// storage before calling this command — the handler only records the metadata.
/// Upload flow: client → POST /api/documents/{id}/upload-url → PUT to S3 → POST /api/documents/{id}/revisions
/// </summary>
public sealed record AddRevisionCommand : IRequest<Result<Guid>>
{
    public required Guid DocumentId { get; init; }
    public required string RevisionNumber { get; init; }
    public required DateOnly IssuedAt { get; init; }
    public required DateOnly EffectiveAt { get; init; }
    public required string StoragePath { get; init; }
    public required long FileSizeBytes { get; init; }
    public required string Sha256Checksum { get; init; }
}

public sealed class AddRevisionCommandValidator : AbstractValidator<AddRevisionCommand>
{
    public AddRevisionCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.RevisionNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.StoragePath).NotEmpty().MaximumLength(500);
        RuleFor(x => x.FileSizeBytes).GreaterThan(0);
        RuleFor(x => x.Sha256Checksum).NotEmpty().Length(64);
        RuleFor(x => x.EffectiveAt).GreaterThanOrEqualTo(x => x.IssuedAt);
    }
}

public sealed class AddRevisionCommandHandler(
    IDocumentRepository documents,
    ICurrentUserService currentUser)
    : IRequestHandler<AddRevisionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddRevisionCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var doc = await documents.GetByIdAsync(request.DocumentId, currentUser.OrganisationId.Value, ct);
        if (doc is null)
            return Result.Failure<Guid>(Error.NotFound(nameof(Domain.Aggregates.Document.MaintenanceDocument), request.DocumentId));

        var actorId = currentUser.UserId!.Value;
        var domainResult = doc.AddRevision(
            request.RevisionNumber,
            request.IssuedAt,
            request.EffectiveAt,
            request.StoragePath,
            request.FileSizeBytes,
            request.Sha256Checksum,
            actorId,
            actorId);

        if (domainResult.IsFailure)
            return Result.Failure<Guid>(Error.Validation(domainResult.ErrorMessage!));

        await documents.UpdateAsync(doc, ct);

        var revision = doc.Revisions.First(r => r.IsCurrent);
        return Result.Success(revision.Id);
    }
}
