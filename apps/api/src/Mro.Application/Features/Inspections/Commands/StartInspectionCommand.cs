using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Inspections.Commands;

public sealed class StartInspectionCommand : IRequest<Result<Unit>>
{
    public required Guid InspectionId { get; init; }
}

public sealed class StartInspectionCommandHandler(
    IInspectionRepository inspections,
    ICurrentUserService currentUser)
    : IRequestHandler<StartInspectionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(StartInspectionCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var inspection = await inspections.GetByIdAsync(
            request.InspectionId, currentUser.OrganisationId.Value, ct);
        if (inspection is null)
            return Result.Failure<Unit>(Error.NotFound("Inspection", request.InspectionId));

        var result = inspection.Start(currentUser.UserId!.Value);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await inspections.UpdateAsync(inspection, ct);
        return Result.Success(Unit.Value);
    }
}
