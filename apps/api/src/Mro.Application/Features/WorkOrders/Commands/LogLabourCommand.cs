using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.WorkOrders.Commands;

public sealed class LogLabourCommand : IRequest<Result<Unit>>
{
    public required Guid WorkOrderId { get; init; }
    public required Guid TaskId { get; init; }
    public required Guid PerformedByUserId { get; init; }
    public required DateTimeOffset StartAt { get; init; }
    public required DateTimeOffset EndAt { get; init; }
    public string? Notes { get; init; }
}

public sealed class LogLabourCommandHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<LogLabourCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(LogLabourCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<Unit>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var result = wo.LogLabour(
            request.TaskId, request.PerformedByUserId,
            request.StartAt, request.EndAt,
            currentUser.UserId!.Value, request.Notes);

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await workOrders.UpdateAsync(wo, ct);
        return Result.Success(Unit.Value);
    }
}
