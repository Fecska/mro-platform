using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Defect.Enums;

namespace Mro.Application.Features.Defects.Commands;

public sealed class DeferDefectCommand : IRequest<Result<Unit>>
{
    public required Guid DefectId { get; init; }
    public required DateTimeOffset DeferredFrom { get; init; }
    public required DateTimeOffset DeferredUntil { get; init; }
    public required Guid ApprovedByUserId { get; init; }
    public required Guid SignedByUserId { get; init; }
    public string? LogReference { get; init; }
    public Guid? StationId { get; init; }
    public required string MelItemNumber { get; init; }
    public required string MelRevision { get; init; }
    public required DeferralCategory DeferralCategory { get; init; }
    public int? OperatorIntervalDays { get; init; }
    public string? OperationalLimitations { get; init; }
    public string? MaintenanceProcedures { get; init; }
}

public sealed class DeferDefectCommandHandler(
    IDefectRepository defects,
    ICurrentUserService currentUser)
    : IRequestHandler<DeferDefectCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeferDefectCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;
        var actorId = currentUser.UserId!.Value;

        var defect = await defects.GetByIdAsync(request.DefectId, orgId, ct);
        if (defect is null)
            return Result.Failure<Unit>(Error.NotFound("Defect", request.DefectId));

        var domainResult = defect.Defer(
            request.DeferredFrom,
            request.DeferredUntil,
            request.ApprovedByUserId,
            request.SignedByUserId,
            request.MelItemNumber,
            request.MelRevision,
            request.DeferralCategory,
            actorId,
            request.OperatorIntervalDays,
            request.LogReference,
            request.StationId,
            request.OperationalLimitations,
            request.MaintenanceProcedures);

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await defects.UpdateAsync(defect, ct);
        return Result.Success(Unit.Value);
    }
}
