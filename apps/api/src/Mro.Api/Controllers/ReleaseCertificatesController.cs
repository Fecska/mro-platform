using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Release.Commands;
using Mro.Application.Features.Release.Queries;
using Mro.Domain.Aggregates.Release.Enums;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/release-certificates")]
[Authorize]
public sealed class ReleaseCertificatesController(ISender sender) : ControllerBase
{
    // ── GET /api/release-certificates ─────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? workOrderId,
        [FromQuery] CertificateStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListCertificatesQuery(workOrderId, status, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/release-certificates/{id} ────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetCertificateQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/release-certificates ────────────────────────────────────

    public sealed record CreateCertificateRequest(
        CertificateType CertificateType,
        Guid WorkOrderId,
        Guid AircraftId,
        string AircraftRegistration,
        string WorkOrderNumber,
        string Scope,
        string RegulatoryBasis,
        Guid CertifyingStaffUserId,
        string? LimitationsAndRemarks);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCertificateRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateCertificateCommand
        {
            CertificateType       = request.CertificateType,
            WorkOrderId           = request.WorkOrderId,
            AircraftId            = request.AircraftId,
            AircraftRegistration  = request.AircraftRegistration,
            WorkOrderNumber       = request.WorkOrderNumber,
            Scope                 = request.Scope,
            RegulatoryBasis       = request.RegulatoryBasis,
            CertifyingStaffUserId = request.CertifyingStaffUserId,
            LimitationsAndRemarks = request.LimitationsAndRemarks,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/release-certificates/{id}/submit ────────────────────────

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new SubmitCertificateCommand { CertificateId = id }, ct);
        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/release-certificates/{id}/sign ──────────────────────────

    public sealed record SignRequest(
        Guid SignerUserId,
        string LicenceRef,
        SignatureMethod Method,
        string StatementText);

    [HttpPost("{id:guid}/sign")]
    public async Task<IActionResult> Sign(Guid id, [FromBody] SignRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new SignCertificateCommand
        {
            CertificateId = id,
            SignerUserId  = request.SignerUserId,
            LicenceRef    = request.LicenceRef,
            Method        = request.Method,
            StatementText = request.StatementText,
            IpAddress     = HttpContext.Connection.RemoteIpAddress?.ToString(),
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/release-certificates/{id}/void ──────────────────────────

    public sealed record VoidRequest(string Reason);

    [HttpPost("{id:guid}/void")]
    public async Task<IActionResult> Void(Guid id, [FromBody] VoidRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new VoidCertificateCommand
        {
            CertificateId = id,
            Reason        = request.Reason,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }
}
