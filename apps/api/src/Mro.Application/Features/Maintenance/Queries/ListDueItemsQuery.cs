using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Maintenance.Dtos;
using Mro.Domain.Aggregates.Maintenance.Enums;

namespace Mro.Application.Features.Maintenance.Queries;

public sealed record ListDueItemsQuery(
    Guid? AircraftId,
    DueStatus? Status,
    DueItemType? Type,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<DueItemSummaryDto>>>;

public sealed class ListDueItemsQueryHandler(
    IDueItemRepository dueItems,
    ICurrentUserService currentUser)
    : IRequestHandler<ListDueItemsQuery, Result<IReadOnlyList<DueItemSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<DueItemSummaryDto>>> Handle(
        ListDueItemsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<DueItemSummaryDto>>(
                Error.Forbidden("Organisation context is required."));

        var list = await dueItems.ListAsync(
            currentUser.OrganisationId.Value, request.AircraftId,
            request.Status, request.Type, request.Page, request.PageSize, ct);

        var dtos = list.Select(d => new DueItemSummaryDto(
            d.Id, d.DueItemRef, d.AircraftId, d.DueItemType, d.IntervalType,
            d.Description, d.Status,
            d.NextDueDate, d.NextDueHours, d.NextDueCycles, d.LastAccomplishedAt))
            .ToList();

        return Result.Success<IReadOnlyList<DueItemSummaryDto>>(dtos);
    }
}
