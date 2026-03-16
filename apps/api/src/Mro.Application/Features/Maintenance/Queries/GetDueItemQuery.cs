using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Maintenance.Dtos;

namespace Mro.Application.Features.Maintenance.Queries;

public sealed record GetDueItemQuery(Guid Id) : IRequest<Result<DueItemDetailDto>>;

public sealed class GetDueItemQueryHandler(
    IDueItemRepository dueItems,
    ICurrentUserService currentUser)
    : IRequestHandler<GetDueItemQuery, Result<DueItemDetailDto>>
{
    public async Task<Result<DueItemDetailDto>> Handle(GetDueItemQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<DueItemDetailDto>(Error.Forbidden("Organisation context is required."));

        var d = await dueItems.GetByIdAsync(request.Id, currentUser.OrganisationId.Value, ct);
        if (d is null)
            return Result.Failure<DueItemDetailDto>(Error.NotFound("DueItem", request.Id));

        return Result.Success(new DueItemDetailDto(
            d.Id, d.DueItemRef, d.AircraftId, d.MaintenanceProgramId,
            d.DueItemType, d.IntervalType, d.Description, d.RegulatoryRef,
            d.IntervalValue, d.IntervalDays, d.ToleranceValue, d.Status,
            d.NextDueDate, d.NextDueHours, d.NextDueCycles,
            d.LastAccomplishedAt, d.LastAccomplishedAtHours, d.LastAccomplishedAtCycles,
            d.LastAccomplishedWorkOrderId));
    }
}
