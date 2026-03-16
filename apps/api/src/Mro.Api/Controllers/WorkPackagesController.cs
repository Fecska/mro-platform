using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Maintenance.Commands;
using Mro.Application.Features.Maintenance.Queries;
using Mro.Domain.Aggregates.Maintenance.Enums;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/work-packages")]
[Authorize]
public sealed class WorkPackagesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? aircraftId,
        [FromQuery] WorkPackageStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListWorkPackagesQuery(aircraftId, status, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetWorkPackageQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record CreatePackageRequest(
        Guid AircraftId,
        string Description,
        DateOnly PlannedStartDate,
        DateOnly? PlannedEndDate,
        Guid? StationId,
        Guid? RelatedWorkOrderId);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePackageRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateWorkPackageCommand
        {
            AircraftId          = request.AircraftId,
            Description         = request.Description,
            PlannedStartDate    = request.PlannedStartDate,
            PlannedEndDate      = request.PlannedEndDate,
            StationId           = request.StationId,
            RelatedWorkOrderId  = request.RelatedWorkOrderId,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record ChangeStatusRequest(WorkPackageStatus NewStatus);

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ChangeWorkPackageStatusCommand
        {
            WorkPackageId = id,
            NewStatus     = request.NewStatus,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record AddItemRequest(
        string Description,
        Guid? DueItemId,
        string? TaskReference,
        decimal? EstimatedManHours);

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddItemRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AddPackageItemCommand
        {
            WorkPackageId      = id,
            Description        = request.Description,
            DueItemId          = request.DueItemId,
            TaskReference      = request.TaskReference,
            EstimatedManHours  = request.EstimatedManHours,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record ItemActionRequest(
        PackageItemAction Action,
        Guid? WorkOrderId,
        decimal? ActualManHours,
        string? Reason);

    [HttpPost("{id:guid}/items/{itemId:guid}/action")]
    public async Task<IActionResult> ItemAction(
        Guid id, Guid itemId,
        [FromBody] ItemActionRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new PackageItemActionCommand
        {
            WorkPackageId  = id,
            ItemId         = itemId,
            Action         = request.Action,
            WorkOrderId    = request.WorkOrderId,
            ActualManHours = request.ActualManHours,
            Reason         = request.Reason,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }
}
