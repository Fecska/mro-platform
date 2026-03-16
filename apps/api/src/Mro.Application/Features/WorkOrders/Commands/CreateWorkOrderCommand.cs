using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.WorkOrder;
using Mro.Domain.Aggregates.WorkOrder.Enums;

namespace Mro.Application.Features.WorkOrders.Commands;

public sealed class CreateWorkOrderCommand : IRequest<Result<Guid>>
{
    public required WorkOrderType WorkOrderType { get; init; }
    public required string Title { get; init; }
    public required Guid AircraftId { get; init; }
    public Guid? StationId { get; init; }
    public DateTimeOffset? PlannedStartAt { get; init; }
    public DateTimeOffset? PlannedEndAt { get; init; }
    public string? CustomerOrderRef { get; init; }
    public Guid? OriginatingDefectId { get; init; }
}

public sealed class CreateWorkOrderCommandHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateWorkOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateWorkOrderCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;
        var actorId = currentUser.UserId!.Value;

        var count = await workOrders.CountAsync(orgId, null, null, null, ct);
        var woNumber = $"WO-{DateTimeOffset.UtcNow.Year}-{(count + 1):D5}";

        var wo = WorkOrder.Create(
            woNumber,
            request.WorkOrderType,
            request.Title,
            request.AircraftId,
            orgId,
            actorId,
            request.StationId,
            request.PlannedStartAt,
            request.PlannedEndAt,
            request.CustomerOrderRef,
            request.OriginatingDefectId);

        await workOrders.AddAsync(wo, ct);
        return Result.Success(wo.Id);
    }
}
