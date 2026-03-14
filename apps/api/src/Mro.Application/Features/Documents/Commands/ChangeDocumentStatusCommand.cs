using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Document.Enums;

namespace Mro.Application.Features.Documents.Commands;

public sealed record ChangeDocumentStatusCommand : IRequest<Result>
{
    public required Guid DocumentId { get; init; }
    public required DocumentStatus NewStatus { get; init; }
    /// <summary>Required when NewStatus is Superseded — the ID of the replacing document.</summary>
    public Guid? SupersededByDocumentId { get; init; }
}

public sealed class ChangeDocumentStatusCommandValidator : AbstractValidator<ChangeDocumentStatusCommand>
{
    public ChangeDocumentStatusCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.SupersededByDocumentId)
            .NotEmpty()
            .When(x => x.NewStatus == DocumentStatus.Superseded)
            .WithMessage("SupersededByDocumentId is required when superseding a document.");
    }
}

public sealed class ChangeDocumentStatusCommandHandler(
    IDocumentRepository documents,
    ICurrentUserService currentUser)
    : IRequestHandler<ChangeDocumentStatusCommand, Result>
{
    public async Task<Result> Handle(ChangeDocumentStatusCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure(Error.Forbidden("Organisation context is required."));

        var doc = await documents.GetByIdAsync(request.DocumentId, currentUser.OrganisationId.Value, ct);
        if (doc is null)
            return Result.Failure(Error.NotFound(nameof(Domain.Aggregates.Document.MaintenanceDocument), request.DocumentId));

        var actorId = currentUser.UserId!.Value;

        var domainResult = request.NewStatus switch
        {
            DocumentStatus.Active     => doc.Activate(actorId),
            DocumentStatus.Superseded => doc.Supersede(request.SupersededByDocumentId!.Value, actorId),
            DocumentStatus.Cancelled  => doc.Cancel(actorId),
            _ => Domain.Application.DomainResult.Failure($"Status '{request.NewStatus}' cannot be set directly.")
        };

        if (domainResult.IsFailure)
            return Result.Failure(Error.Validation(domainResult.ErrorMessage!));

        await documents.UpdateAsync(doc, ct);
        return Result.Success();
    }
}
