using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Employees.Commands;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/authorisations")]
[Authorize]
public sealed class AuthorisationsController(ISender sender) : ControllerBase
{
    // ── PATCH /api/authorisations/{id} ─────────────────────────────────────

    public sealed record AmendAuthorisationRequest(
        DateOnly? ValidUntil,
        bool ClearValidUntil,
        string? AircraftTypes,
        string? ComponentScope,
        string? StationScope,
        string? IssuingAuthority);

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Amend(Guid id, [FromBody] AmendAuthorisationRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AmendAuthorisationCommand
        {
            AuthorisationId  = id,
            ValidUntil       = request.ValidUntil,
            ClearValidUntil  = request.ClearValidUntil,
            AircraftTypes    = request.AircraftTypes,
            ComponentScope   = request.ComponentScope,
            StationScope     = request.StationScope,
            IssuingAuthority = request.IssuingAuthority,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/authorisations/{id}/suspend ──────────────────────────────

    public sealed record SuspendRequest(string Reason);

    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] SuspendRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new SuspendAuthorisationCommand
        {
            AuthorisationId = id,
            Reason          = request.Reason,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/authorisations/{id}/resume ───────────────────────────────

    [HttpPost("{id:guid}/resume")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new ResumeAuthorisationCommand { AuthorisationId = id }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }
}
