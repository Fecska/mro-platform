using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Employees.Commands;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/licences")]
[Authorize]
public sealed class LicencesController(ISender sender) : ControllerBase
{
    // ── PATCH /api/licences/{id} ───────────────────────────────────────────

    public sealed record UpdateLicenceRequest(
        DateOnly? ExpiresAt,
        bool ClearExpiry,
        string? ScopeNotes,
        string? AttachmentRef);

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLicenceRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateLicenceCommand
        {
            LicenceId     = id,
            ExpiresAt     = request.ExpiresAt,
            ClearExpiry   = request.ClearExpiry,
            ScopeNotes    = request.ScopeNotes,
            AttachmentRef = request.AttachmentRef,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }
}
