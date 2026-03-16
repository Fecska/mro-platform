using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Documents.Dtos;

namespace Mro.Application.Features.Documents.Commands;

// ── Command ──────────────────────────────────────────────────────────────────

public sealed record AddEffectivityCommand : IRequest<Result<Guid>>
{
    public required Guid DocumentId { get; init; }
    /// <summary>ICAO type code, e.g. "B738". Null = all aircraft types.</summary>
    public string? IcaoTypeCode { get; init; }
    public string? SerialFrom { get; init; }
    public string? SerialTo { get; init; }
    public string? ConditionNote { get; init; }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class AddEffectivityCommandValidator : AbstractValidator<AddEffectivityCommand>
{
    public AddEffectivityCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.IcaoTypeCode).MaximumLength(10).When(x => x.IcaoTypeCode is not null);
        RuleFor(x => x.SerialFrom).MaximumLength(20).When(x => x.SerialFrom is not null);
        RuleFor(x => x.SerialTo).MaximumLength(20).When(x => x.SerialTo is not null);
        RuleFor(x => x.ConditionNote).MaximumLength(500).When(x => x.ConditionNote is not null);
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class AddEffectivityCommandHandler(
    IDocumentRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<AddEffectivityCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddEffectivityCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var doc = await repository.GetByIdAsync(request.DocumentId, currentUser.OrganisationId.Value, ct);
        if (doc is null)
            return Result.Failure<Guid>(
                Error.NotFound(nameof(Domain.Aggregates.Document.MaintenanceDocument), request.DocumentId));

        var actorId = currentUser.UserId!.Value;

        doc.AddEffectivity(
            request.IcaoTypeCode,
            request.SerialFrom,
            request.SerialTo,
            request.ConditionNote,
            actorId);

        await repository.UpdateAsync(doc, ct);

        // Return the ID of the newly added effectivity (last one added)
        var newEffectivity = doc.Effectivities.Last();
        return Result.Success(newEffectivity.Id);
    }
}
