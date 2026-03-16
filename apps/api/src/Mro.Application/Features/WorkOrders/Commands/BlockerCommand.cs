using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.WorkOrder.Enums;

namespace Mro.Application.Features.WorkOrders.Commands;

// ── Raise blocker ────────────────────────────────────────────────────────────

public sealed class RaiseBlockerCommand : IRequest<Result<Unit>>
{
    public required Guid WorkOrderId { get; init; }
    public required BlockerType BlockerType { get; init; }
    public required string Description { get; init; }
    public required WorkOrderStatus WaitingStatus { get; init; }
}

public sealed class RaiseBlockerCommandHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<RaiseBlockerCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RaiseBlockerCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<Unit>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var actorId = currentUser.UserId!.Value;
        var result = wo.RaiseBlocker(request.BlockerType, request.Description, actorId, request.WaitingStatus, actorId);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await workOrders.UpdateAsync(wo, ct);
        return Result.Success(Unit.Value);
    }
}

// ── Resolve blocker ──────────────────────────────────────────────────────────

public sealed class ResolveBlockerCommand : IRequest<Result<Unit>>
{
    public required Guid WorkOrderId { get; init; }
    public required Guid BlockerId { get; init; }
    public required string ResolutionNote { get; init; }
}

public sealed class ResolveBlockerCommandHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<ResolveBlockerCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ResolveBlockerCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<Unit>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var actorId = currentUser.UserId!.Value;
        var result = wo.ResolveBlocker(request.BlockerId, request.ResolutionNote, actorId, actorId);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await workOrders.UpdateAsync(wo, ct);
        return Result.Success(Unit.Value);
    }
}
