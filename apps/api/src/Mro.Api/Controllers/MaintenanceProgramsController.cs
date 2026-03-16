using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Maintenance.Commands;
using Mro.Application.Features.Maintenance.Queries;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/maintenance-programs")]
[Authorize]
public sealed class MaintenanceProgramsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? aircraftTypeCode, CancellationToken ct)
    {
        var result = await sender.Send(new ListMaintenanceProgramsQuery(aircraftTypeCode), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    public sealed record CreateProgramRequest(
        string ProgramNumber,
        string AircraftTypeCode,
        string Title,
        string RevisionNumber,
        DateOnly RevisionDate,
        string? ApprovalReference);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProgramRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateMaintenanceProgramCommand
        {
            ProgramNumber     = request.ProgramNumber,
            AircraftTypeCode  = request.AircraftTypeCode,
            Title             = request.Title,
            RevisionNumber    = request.RevisionNumber,
            RevisionDate      = request.RevisionDate,
            ApprovalReference = request.ApprovalReference,
        }, ct);

        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }
}
