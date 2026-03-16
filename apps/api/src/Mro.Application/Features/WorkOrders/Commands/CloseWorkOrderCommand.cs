using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.WorkOrders.Commands;

public sealed class CloseWorkOrderCommand : IRequest<Result<Unit>>
{
    public required Guid WorkOrderId { get; init; }
}

public sealed class CloseWorkOrderCommandHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<CloseWorkOrderCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CloseWorkOrderCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<Unit>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var domainResult = wo.Close(currentUser.UserId!.Value);
        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await workOrders.UpdateAsync(wo, ct);
        return Result.Success(Unit.Value);
    }
}
