using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.WorkOrders.Dtos;
using Mro.Domain.Aggregates.WorkOrder.Enums;

namespace Mro.Application.Features.WorkOrders.Queries;

public sealed record ListWorkOrdersQuery(
    Guid? AircraftId = null,
    WorkOrderStatus? Status = null,
    WorkOrderType? Type = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<ListWorkOrdersResult>>;

public sealed record ListWorkOrdersResult(
    IReadOnlyList<WorkOrderSummaryDto> Items,
    int Total,
    int Page,
    int PageSize);

public sealed class ListWorkOrdersQueryHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<ListWorkOrdersQuery, Result<ListWorkOrdersResult>>
{
    public async Task<Result<ListWorkOrdersResult>> Handle(ListWorkOrdersQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<ListWorkOrdersResult>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;
        var items = await workOrders.ListAsync(orgId, request.AircraftId, request.Status, request.Type, request.Page, request.PageSize, ct);
        var total = await workOrders.CountAsync(orgId, request.AircraftId, request.Status, request.Type, ct);

        var dtos = items.Select(wo => new WorkOrderSummaryDto(
            wo.Id,
            wo.WoNumber,
            wo.WorkOrderType.ToString(),
            wo.Title,
            wo.Status.ToString(),
            wo.AircraftId,
            wo.StationId,
            wo.PlannedStartAt,
            wo.PlannedEndAt,
            wo.Tasks.Count(t => t.Status != WorkOrderTaskStatus.Cancelled),
            wo.Tasks.Count(t => t.IsSignedOff),
            wo.CreatedAt)).ToList();

        return Result.Success(new ListWorkOrdersResult(dtos, total, request.Page, request.PageSize));
    }
}
