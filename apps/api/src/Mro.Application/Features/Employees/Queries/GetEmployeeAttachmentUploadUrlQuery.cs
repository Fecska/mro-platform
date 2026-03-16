using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Queries;

public sealed class GetEmployeeAttachmentUploadUrlQuery : IRequest<Result<EmployeeAttachmentUploadUrlDto>>
{
    public required Guid EmployeeId { get; init; }

    /// <summary>
    /// Original file name supplied by the client — used to extract the extension.
    /// E.g. "licence-b1.pdf" → extension ".pdf"
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>MIME type of the file (e.g. "application/pdf", "image/jpeg").</summary>
    public required string ContentType { get; init; }
}

public sealed record EmployeeAttachmentUploadUrlDto(
    /// <summary>Pre-signed PUT URL — client uploads the file directly to blob storage.</summary>
    string UploadUrl,
    /// <summary>Storage path to pass back when calling RegisterEmployeeAttachmentCommand.</summary>
    string StoragePath,
    /// <summary>Pre-generated attachment ID to use in RegisterEmployeeAttachmentCommand.</summary>
    Guid AttachmentId);

public sealed class GetEmployeeAttachmentUploadUrlQueryHandler(
    IEmployeeRepository employees,
    IDocumentStorageService storage,
    ICurrentUserService currentUser)
    : IRequestHandler<GetEmployeeAttachmentUploadUrlQuery, Result<EmployeeAttachmentUploadUrlDto>>
{
    public async Task<Result<EmployeeAttachmentUploadUrlDto>> Handle(
        GetEmployeeAttachmentUploadUrlQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<EmployeeAttachmentUploadUrlDto>(
                Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        var exists = await employees.GetByIdAsync(request.EmployeeId, orgId, ct);
        if (exists is null)
            return Result.Failure<EmployeeAttachmentUploadUrlDto>(
                Error.NotFound("Employee", request.EmployeeId));

        var attachmentId = Guid.NewGuid();
        var extension    = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension)) extension = ".bin";

        var storagePath = storage.BuildEmployeeAttachmentPath(
            orgId, request.EmployeeId, attachmentId, extension);

        var uploadUrl = await storage.GetUploadUrlAsync(
            storagePath, request.ContentType, expirySeconds: 300, ct);

        return Result.Success(new EmployeeAttachmentUploadUrlDto(uploadUrl, storagePath, attachmentId));
    }
}
