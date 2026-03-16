using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Inspections.Dtos;

namespace Mro.Application.Features.Inspections.Queries;

public sealed record GetInspectionQuery(Guid Id) : IRequest<Result<InspectionDetailDto>>;

public sealed class GetInspectionQueryHandler(
    IInspectionRepository inspections,
    ICurrentUserService currentUser)
    : IRequestHandler<GetInspectionQuery, Result<InspectionDetailDto>>
{
    public async Task<Result<InspectionDetailDto>> Handle(GetInspectionQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<InspectionDetailDto>(Error.Forbidden("Organisation context is required."));

        var i = await inspections.GetByIdAsync(request.Id, currentUser.OrganisationId.Value, ct);
        if (i is null)
            return Result.Failure<InspectionDetailDto>(Error.NotFound("Inspection", request.Id));

        return Result.Success(new InspectionDetailDto(
            i.Id, i.InspectionNumber, i.WorkOrderId, i.WorkOrderTaskId, i.AircraftId,
            i.InspectionType, i.Status, i.InspectorUserId,
            i.ScheduledAt, i.StartedAt, i.CompletedAt,
            i.Findings, i.OutcomeRemarks, i.WaiverReason, i.CreatedAt));
    }
}
