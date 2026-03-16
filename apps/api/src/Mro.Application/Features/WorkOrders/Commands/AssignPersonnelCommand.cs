using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.WorkOrder.Enums;

namespace Mro.Application.Features.WorkOrders.Commands;

public sealed class AssignPersonnelCommand : IRequest<Result<Unit>>
{
    public required Guid WorkOrderId { get; init; }
    public required Guid UserId { get; init; }
    public required AssignmentRole Role { get; init; }
}

public sealed class AssignPersonnelCommandHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<AssignPersonnelCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AssignPersonnelCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<Unit>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var actorId = currentUser.UserId!.Value;

        var result = wo.AssignPersonnel(request.UserId, request.Role, actorId, actorId);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await workOrders.UpdateAsync(wo, ct);
        return Result.Success(Unit.Value);
    }
}
