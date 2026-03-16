using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Inspections.Commands;

public sealed class RecordInspectionOutcomeCommand : IRequest<Result<Unit>>
{
    public required Guid InspectionId { get; init; }
    public required bool Passed { get; init; }
    public required string Remarks { get; init; }
    public string? Findings { get; init; }
}

public sealed class RecordInspectionOutcomeCommandHandler(
    IInspectionRepository inspections,
    ICurrentUserService currentUser)
    : IRequestHandler<RecordInspectionOutcomeCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RecordInspectionOutcomeCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var inspection = await inspections.GetByIdAsync(
            request.InspectionId, currentUser.OrganisationId.Value, ct);
        if (inspection is null)
            return Result.Failure<Unit>(Error.NotFound("Inspection", request.InspectionId));

        var result = inspection.RecordOutcome(
            request.Passed, request.Remarks, currentUser.UserId!.Value, request.Findings);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await inspections.UpdateAsync(inspection, ct);
        return Result.Success(Unit.Value);
    }
}
