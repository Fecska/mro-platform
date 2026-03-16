using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.WorkOrders.Commands;

/// <summary>
/// Transitions the work order to WaitingCertification.
/// Hard Stop HS-009b: all active tasks must be SignedOff.
/// </summary>
public sealed class RequestReleaseCommand : IRequest<Result<Unit>>
{
    public required Guid WorkOrderId { get; init; }
}

public sealed class RequestReleaseCommandHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<RequestReleaseCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RequestReleaseCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<Unit>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var domainResult = wo.SubmitForCertification(currentUser.UserId!.Value);
        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await workOrders.UpdateAsync(wo, ct);
        return Result.Success(Unit.Value);
    }
}
