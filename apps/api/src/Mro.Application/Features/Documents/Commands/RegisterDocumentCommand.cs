using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Document;
using Mro.Domain.Aggregates.Document.Enums;

namespace Mro.Application.Features.Documents.Commands;

public sealed record RegisterDocumentCommand : IRequest<Result<Guid>>
{
    public required string DocumentNumber { get; init; }
    public required DocumentType DocumentType { get; init; }
    public required string Title { get; init; }
    public required string Issuer { get; init; }
    public string? RegulatoryReference { get; init; }
    public Guid? SupersedesDocumentId { get; init; }
}

public sealed class RegisterDocumentCommandValidator : AbstractValidator<RegisterDocumentCommand>
{
    public RegisterDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Issuer).NotEmpty().MaximumLength(150);
    }
}

public sealed class RegisterDocumentCommandHandler(
    IDocumentRepository documents,
    ICurrentUserService currentUser)
    : IRequestHandler<RegisterDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterDocumentCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        if (await documents.ExistsAsync(request.DocumentNumber, request.DocumentType, orgId, ct))
            return Result.Failure<Guid>(
                Error.Conflict($"Document '{request.DocumentNumber}' of type '{request.DocumentType}' already exists."));

        var doc = MaintenanceDocument.Register(
            request.DocumentNumber,
            request.DocumentType,
            request.Title,
            request.Issuer,
            orgId,
            currentUser.UserId!.Value,
            request.RegulatoryReference,
            request.SupersedesDocumentId);

        await documents.AddAsync(doc, ct);
        return Result.Success(doc.Id);
    }
}
