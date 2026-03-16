using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Inspections.Dtos;
using Mro.Domain.Aggregates.Inspection.Enums;

namespace Mro.Application.Features.Inspections.Queries;

public sealed record ListInspectionsQuery(
    Guid? WorkOrderId,
    InspectionStatus? Status,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<InspectionSummaryDto>>>;

public sealed class ListInspectionsQueryHandler(
    IInspectionRepository inspections,
    ICurrentUserService currentUser)
    : IRequestHandler<ListInspectionsQuery, Result<IReadOnlyList<InspectionSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<InspectionSummaryDto>>> Handle(
        ListInspectionsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<InspectionSummaryDto>>(
                Error.Forbidden("Organisation context is required."));

        var list = await inspections.ListAsync(
            currentUser.OrganisationId.Value, request.WorkOrderId, request.Status,
            request.Page, request.PageSize, ct);

        var dtos = list.Select(i => new InspectionSummaryDto(
            i.Id, i.InspectionNumber, i.WorkOrderId, i.WorkOrderTaskId, i.AircraftId,
            i.InspectionType, i.Status, i.InspectorUserId, i.ScheduledAt, i.CompletedAt))
            .ToList();

        return Result.Success<IReadOnlyList<InspectionSummaryDto>>(dtos);
    }
}
