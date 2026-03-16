using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Defects.Commands;

public sealed record CloseDefectCommand : IRequest<Result>
{
    public required Guid DefectId { get; init; }
    public required string Reason { get; init; }
}

public sealed class CloseDefectCommandValidator : AbstractValidator<CloseDefectCommand>
{
    public CloseDefectCommandValidator()
    {
        RuleFor(x => x.DefectId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public sealed class CloseDefectCommandHandler(
    IDefectRepository defects,
    ICurrentUserService currentUser)
    : IRequestHandler<CloseDefectCommand, Result>
{
    public async Task<Result> Handle(CloseDefectCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure(Error.Forbidden("Organisation context is required."));

        var defect = await defects.GetByIdAsync(request.DefectId, currentUser.OrganisationId.Value, ct);
        if (defect is null)
            return Result.Failure(Error.NotFound("Defect", request.DefectId));

        var domainResult = defect.Close(request.Reason, currentUser.UserId!.Value);
        if (domainResult.IsFailure)
            return Result.Failure(Error.Validation(domainResult.ErrorMessage!));

        await defects.UpdateAsync(defect, ct);
        return Result.Success();
    }
}
