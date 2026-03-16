using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Defect.Enums;

namespace Mro.Application.Features.Defects.Commands;

public sealed record UpdateDefectCommand : IRequest<Result>
{
    public required Guid DefectId { get; init; }
    public string? Description { get; init; }
    public string? AtaChapter { get; init; }
    public DefectSeverity? Severity { get; init; }
}

public sealed class UpdateDefectCommandValidator : AbstractValidator<UpdateDefectCommand>
{
    public UpdateDefectCommandValidator()
    {
        RuleFor(x => x.DefectId).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.AtaChapter).MaximumLength(20).When(x => x.AtaChapter is not null);
    }
}

public sealed class UpdateDefectCommandHandler(
    IDefectRepository defects,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateDefectCommand, Result>
{
    public async Task<Result> Handle(UpdateDefectCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure(Error.Forbidden("Organisation context is required."));

        var defect = await defects.GetByIdAsync(request.DefectId, currentUser.OrganisationId.Value, ct);
        if (defect is null)
            return Result.Failure(Error.NotFound("Defect", request.DefectId));

        var domainResult = defect.UpdateDetails(
            request.Description,
            request.AtaChapter,
            request.Severity,
            currentUser.UserId!.Value);

        if (domainResult.IsFailure)
            return Result.Failure(Error.Validation(domainResult.ErrorMessage!));

        await defects.UpdateAsync(defect, ct);
        return Result.Success();
    }
}
