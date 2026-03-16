using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Defects.Dtos;

namespace Mro.Application.Features.Defects.Queries;

public sealed record GetDefectQuery(Guid Id) : IRequest<Result<DefectDetailDto>>;

public sealed class GetDefectQueryHandler(
    IDefectRepository defects,
    ICurrentUserService currentUser)
    : IRequestHandler<GetDefectQuery, Result<DefectDetailDto>>
{
    public async Task<Result<DefectDetailDto>> Handle(GetDefectQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<DefectDetailDto>(Error.Forbidden("Organisation context is required."));

        var defect = await defects.GetByIdAsync(request.Id, currentUser.OrganisationId.Value, ct);
        if (defect is null)
            return Result.Failure<DefectDetailDto>(Error.NotFound("Defect", request.Id));

        var actions = defect.Actions.Select(a => new DefectActionDto(
            a.Id, a.ActionType.ToString(), a.Description,
            a.PerformedByUserId, a.PerformedAt,
            a.AtaReference, a.PartNumber, a.SerialNumber, a.WorkOrderId)).ToList();

        var active = defect.ActiveDeferral;
        var deferralDto = active is null ? null : new DeferredDefectDto(
            active.Id, active.DeferredFrom, active.DeferredUntil,
            active.ApprovedByUserId, active.SignedByUserId,
            active.LogReference, active.IsExpired);

        return Result.Success(new DefectDetailDto(
            defect.Id,
            defect.DefectNumber,
            defect.AircraftId,
            defect.Status.ToString(),
            defect.Severity.ToString(),
            defect.Source.ToString(),
            defect.AtaChapter,
            defect.Description,
            defect.DiscoveredAt,
            defect.DiscoveredAtStationId,
            defect.IsAdMandated,
            defect.AdDocumentId,
            defect.AssignedToUserId,
            defect.WorkOrderId,
            defect.ClosureReason,
            actions,
            deferralDto,
            defect.CreatedAt,
            defect.CreatedBy));
    }
}
