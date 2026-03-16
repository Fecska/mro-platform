using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.WorkOrders.Dtos;

namespace Mro.Application.Features.WorkOrders.Queries;

public sealed record GetWorkOrderQuery(Guid Id) : IRequest<Result<WorkOrderDetailDto>>;

public sealed class GetWorkOrderQueryHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<GetWorkOrderQuery, Result<WorkOrderDetailDto>>
{
    public async Task<Result<WorkOrderDetailDto>> Handle(GetWorkOrderQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<WorkOrderDetailDto>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.Id, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<WorkOrderDetailDto>(Error.NotFound("WorkOrder", request.Id));

        var tasks = wo.Tasks.Select(t => new WorkOrderTaskDto(
            t.Id, t.TaskNumber, t.Title, t.AtaChapter, t.Description,
            t.Status.ToString(), t.RequiredLicence,
            t.EstimatedHours, t.TotalHoursLogged,
            t.IsSignedOff, t.SignedOffByUserId, t.SignedOffAt, t.DocumentId,
            t.LabourEntries.Select(l => new LabourEntryDto(
                l.Id, l.PerformedByUserId, l.StartAt, l.EndAt, l.Hours, l.Notes)).ToList(),
            t.RequiredParts.Select(p => new RequiredPartDto(
                p.Id, p.PartNumber, p.Description, p.QuantityRequired,
                p.UnitOfMeasure, p.IssueSlipRef, p.IssuedQuantity, p.IsFullyIssued)).ToList(),
            t.RequiredTools.Select(r => new RequiredToolDto(
                r.Id, r.ToolNumber, r.Description, r.CalibratedExpiry,
                r.IsCheckedOut, r.CheckedOutAt, r.IsCalibrationExpired)).ToList()
        )).ToList();

        var assignments = wo.Assignments.Select(a => new WorkOrderAssignmentDto(
            a.UserId, a.Role.ToString(), a.AssignedAt)).ToList();

        var blockers = wo.ActiveBlockers.Select(b => new WorkOrderBlockerDto(
            b.Id, b.BlockerType.ToString(), b.Description, b.RaisedByUserId, b.RaisedAt)).ToList();

        return Result.Success(new WorkOrderDetailDto(
            wo.Id, wo.WoNumber, wo.WorkOrderType.ToString(), wo.Title, wo.Status.ToString(),
            wo.AircraftId, wo.StationId,
            wo.PlannedStartAt, wo.PlannedEndAt,
            wo.ActualStartAt, wo.ActualEndAt,
            wo.CustomerOrderRef, wo.OriginatingDefectId,
            wo.AllTasksSignedOff,
            tasks, assignments, blockers,
            wo.CreatedAt, wo.CreatedBy));
    }
}
