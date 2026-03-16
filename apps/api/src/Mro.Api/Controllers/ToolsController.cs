using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Tools.Commands;
using Mro.Application.Features.Tools.Queries;
using Mro.Domain.Aggregates.Tool.Enums;
using Mro.Application.Features.Tools.Dtos;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/tools")]
[Authorize]
public sealed class ToolsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] ToolStatus? status,
        [FromQuery] ToolCategory? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListToolsQuery(status, category, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetToolQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record CreateToolRequest(
        string ToolNumber,
        string Description,
        ToolCategory Category,
        bool CalibrationRequired,
        string? Location);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateToolRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateToolCommand
        {
            ToolNumber          = request.ToolNumber,
            Description         = request.Description,
            Category            = request.Category,
            CalibrationRequired = request.CalibrationRequired,
            Location            = request.Location,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record CheckOutRequest(Guid WorkOrderTaskId, Guid CheckedOutByUserId);

    [HttpPost("{id:guid}/checkout")]
    [HttpPost("{id:guid}/issue")]
    public async Task<IActionResult> CheckOut(Guid id, [FromBody] CheckOutRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CheckOutToolCommand
        {
            ToolId             = id,
            WorkOrderTaskId    = request.WorkOrderTaskId,
            CheckedOutByUserId = request.CheckedOutByUserId,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    [HttpPost("{id:guid}/checkin")]
    [HttpPost("{id:guid}/return")]
    public async Task<IActionResult> CheckIn(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new CheckInToolCommand { ToolId = id }, ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    [HttpPost("{id:guid}/send-for-calibration")]
    public async Task<IActionResult> SendForCalibration(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new SendForCalibrationCommand { ToolId = id }, ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record RecordCalibrationRequest(
        DateTimeOffset CalibratedAt,
        DateTimeOffset ExpiresAt,
        string PerformedBy,
        string? CertificateRef,
        string? Notes);

    [HttpPost("{id:guid}/calibrations")]
    [HttpPost("{id:guid}/calibration")]
    public async Task<IActionResult> RecordCalibration(Guid id, [FromBody] RecordCalibrationRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RecordCalibrationCommand
        {
            ToolId         = id,
            CalibratedAt   = request.CalibratedAt,
            ExpiresAt      = request.ExpiresAt,
            PerformedBy    = request.PerformedBy,
            CertificateRef = request.CertificateRef,
            Notes          = request.Notes,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    [HttpPost("{id:guid}/retire")]
    public async Task<IActionResult> Retire(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new RetireToolCommand { ToolId = id }, ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/tools/{id}/calibrations ──────────────────────────────────

    [HttpGet("{id:guid}/calibrations")]
    public async Task<IActionResult> ListCalibrations(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new ListCalibrationsQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }
}
