using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Defect.Enums;

namespace Mro.Application.Features.Defects.Commands;

public sealed class RecordDefectActionCommand : IRequest<Result<Unit>>
{
    public required Guid DefectId { get; init; }
    public required ActionType ActionType { get; init; }
    public required string Description { get; init; }
    public required Guid PerformedByUserId { get; init; }
    public required DateTimeOffset PerformedAt { get; init; }
    public string? AtaReference { get; init; }
    public string? PartNumber { get; init; }
    public string? SerialNumber { get; init; }
    public Guid? WorkOrderId { get; init; }
}

public sealed class RecordDefectActionCommandHandler(
    IDefectRepository defects,
    ICurrentUserService currentUser)
    : IRequestHandler<RecordDefectActionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RecordDefectActionCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var defect = await defects.GetByIdAsync(request.DefectId, currentUser.OrganisationId.Value, ct);
        if (defect is null)
            return Result.Failure<Unit>(Error.NotFound("Defect", request.DefectId));

        var result = defect.RecordAction(
            request.ActionType,
            request.Description,
            request.PerformedByUserId,
            request.PerformedAt,
            currentUser.UserId!.Value,
            request.AtaReference,
            request.PartNumber,
            request.SerialNumber,
            request.WorkOrderId);

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await defects.UpdateAsync(defect, ct);
        return Result.Success(Unit.Value);
    }
}
