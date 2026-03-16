using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Defect.Enums;
using DomainResult = Mro.Domain.Application.DomainResult;

namespace Mro.Application.Features.Defects.Commands;

/// <summary>
/// Handles simple status transitions that don't require additional data:
///   Triaged → Open (Accept)
///   RectificationInProgress → InspectionPending (SubmitForInspection)
/// For Triage, Defer, Clear, and Close use the dedicated commands.
/// </summary>
public sealed class ChangeDefectStatusCommand : IRequest<Result<Unit>>
{
    public required Guid DefectId { get; init; }
    public required DefectStatus NewStatus { get; init; }
    public Guid? WorkOrderId { get; init; }
    public Guid? CertifyingStaffUserId { get; init; }
    public string? ClosureReason { get; init; }
}

public sealed class ChangeDefectStatusCommandHandler(
    IDefectRepository defects,
    ICurrentUserService currentUser)
    : IRequestHandler<ChangeDefectStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ChangeDefectStatusCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var defect = await defects.GetByIdAsync(request.DefectId, currentUser.OrganisationId.Value, ct);
        if (defect is null)
            return Result.Failure<Unit>(Error.NotFound("Defect", request.DefectId));

        var actorId = currentUser.UserId!.Value;

        var domainResult = request.NewStatus switch
        {
            DefectStatus.Open =>
                defect.Accept(actorId),

            DefectStatus.RectificationInProgress =>
                request.WorkOrderId.HasValue
                    ? defect.StartRectification(request.WorkOrderId.Value, actorId)
                    : DomainResult.Failure("WorkOrderId is required to start rectification."),

            DefectStatus.InspectionPending =>
                defect.SubmitForInspection(actorId),

            DefectStatus.Cleared =>
                request.CertifyingStaffUserId.HasValue
                    ? defect.Clear(request.CertifyingStaffUserId.Value, actorId)
                    : DomainResult.Failure("CertifyingStaffUserId is required to clear a defect."),

            DefectStatus.Closed =>
                !string.IsNullOrWhiteSpace(request.ClosureReason)
                    ? defect.Close(request.ClosureReason, actorId)
                    : DomainResult.Failure("ClosureReason is required to close a defect."),

            _ => DomainResult.Failure($"Use the dedicated command for '{request.NewStatus}' transitions."),
        };

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await defects.UpdateAsync(defect, ct);
        return Result.Success(Unit.Value);
    }
}
