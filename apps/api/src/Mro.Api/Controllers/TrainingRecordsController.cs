using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Employees.Commands;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/training-records")]
[Authorize]
public sealed class TrainingRecordsController(ISender sender) : ControllerBase
{
    // ── PATCH /api/training-records/{id} ──────────────────────────────────

    public sealed record UpdateTrainingRecordRequest(
        DateOnly? ExpiresAt,
        bool ClearExpiresAt,
        string? Result,
        string? CertificateRef);

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTrainingRecordRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateTrainingRecordCommand
        {
            TrainingRecordId = id,
            ExpiresAt        = request.ExpiresAt,
            ClearExpiresAt   = request.ClearExpiresAt,
            Result           = request.Result,
            CertificateRef   = request.CertificateRef,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }
}
