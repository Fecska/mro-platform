using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Documents.Commands;
using Mro.Application.Features.Documents.Queries;
using Mro.Domain.Aggregates.Document.Enums;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public sealed class DocumentsController(ISender sender) : ControllerBase
{
    // ── GET /api/documents ────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] DocumentType? type,
        [FromQuery] DocumentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListDocumentsQuery(type, status, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/documents/{id} ───────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetDocumentQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/documents/{id}/revisions/{revisionId}/download-url ───────

    [HttpGet("{id:guid}/revisions/{revisionId:guid}/download-url")]
    public async Task<IActionResult> GetDownloadUrl(Guid id, Guid revisionId, CancellationToken ct)
    {
        var result = await sender.Send(new GetDownloadUrlQuery(id, revisionId), ct);
        return result.IsSuccess ? Ok(new { url = result.Value })
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/documents ───────────────────────────────────────────────

    public sealed record RegisterDocumentRequest(
        string DocumentNumber,
        DocumentType DocumentType,
        string Title,
        string Issuer,
        string? RegulatoryReference,
        Guid? SupersedesDocumentId);

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterDocumentRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RegisterDocumentCommand
        {
            DocumentNumber = request.DocumentNumber,
            DocumentType = request.DocumentType,
            Title = request.Title,
            Issuer = request.Issuer,
            RegulatoryReference = request.RegulatoryReference,
            SupersedesDocumentId = request.SupersedesDocumentId,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/documents/{id}/revisions ────────────────────────────────

    public sealed record AddRevisionRequest(
        string RevisionNumber,
        DateOnly IssuedAt,
        DateOnly EffectiveAt,
        string StoragePath,
        long FileSizeBytes,
        string Sha256Checksum);

    [HttpPost("{id:guid}/revisions")]
    public async Task<IActionResult> AddRevision(Guid id, [FromBody] AddRevisionRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AddRevisionCommand
        {
            DocumentId = id,
            RevisionNumber = request.RevisionNumber,
            IssuedAt = request.IssuedAt,
            EffectiveAt = request.EffectiveAt,
            StoragePath = request.StoragePath,
            FileSizeBytes = request.FileSizeBytes,
            Sha256Checksum = request.Sha256Checksum,
        }, ct);

        return result.IsSuccess ? Ok(new { id = result.Value }) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/documents/{id}/status ───────────────────────────────────

    public sealed record ChangeStatusRequest(DocumentStatus NewStatus, Guid? SupersededByDocumentId);

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new ChangeDocumentStatusCommand
        {
            DocumentId = id,
            NewStatus = request.NewStatus,
            SupersededByDocumentId = request.SupersededByDocumentId,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }
}
