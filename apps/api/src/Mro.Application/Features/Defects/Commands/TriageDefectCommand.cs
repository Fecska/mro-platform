using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Defect.Enums;

namespace Mro.Application.Features.Defects.Commands;

public sealed class TriageDefectCommand : IRequest<Result<Unit>>
{
    public required Guid DefectId { get; init; }
    public required Guid AssignedToUserId { get; init; }
    public DefectSeverity? OverrideSeverity { get; init; }
}

public sealed class TriageDefectCommandHandler(
    IDefectRepository defects,
    ICurrentUserService currentUser)
    : IRequestHandler<TriageDefectCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(TriageDefectCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var defect = await defects.GetByIdAsync(request.DefectId, currentUser.OrganisationId.Value, ct);
        if (defect is null)
            return Result.Failure<Unit>(Error.NotFound("Defect", request.DefectId));

        var result = defect.Triage(request.AssignedToUserId, request.OverrideSeverity, currentUser.UserId!.Value);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await defects.UpdateAsync(defect, ct);
        return Result.Success(Unit.Value);
    }
}
