using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Queries;

public sealed class ListEmployeeAttachmentsQuery : IRequest<Result<IReadOnlyList<EmployeeAttachmentDto>>>
{
    public required Guid EmployeeId { get; init; }
    public AttachmentType? Type { get; init; }
}

public sealed record EmployeeAttachmentDto(
    Guid Id,
    AttachmentType AttachmentType,
    string DisplayName,
    string ContentType,
    long FileSizeBytes,
    Guid? LinkedEntityId,
    DateTimeOffset CreatedAt,
    /// <summary>Time-limited pre-signed download URL (5 minutes).</summary>
    string DownloadUrl);

public sealed class ListEmployeeAttachmentsQueryHandler(
    IEmployeeRepository employees,
    IDocumentStorageService storage,
    ICurrentUserService currentUser)
    : IRequestHandler<ListEmployeeAttachmentsQuery, Result<IReadOnlyList<EmployeeAttachmentDto>>>
{
    public async Task<Result<IReadOnlyList<EmployeeAttachmentDto>>> Handle(
        ListEmployeeAttachmentsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<EmployeeAttachmentDto>>(
                Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        var emp = await employees.GetByIdAsync(request.EmployeeId, orgId, ct);
        if (emp is null)
            return Result.Failure<IReadOnlyList<EmployeeAttachmentDto>>(
                Error.NotFound("Employee", request.EmployeeId));

        var attachments = await employees.ListAttachmentsAsync(
            request.EmployeeId, orgId, request.Type, ct);

        var dtos = new List<EmployeeAttachmentDto>(attachments.Count);
        foreach (var a in attachments)
        {
            var url = await storage.GetDownloadUrlAsync(a.StoragePath, expirySeconds: 300, ct);
            dtos.Add(new EmployeeAttachmentDto(
                a.Id, a.AttachmentType, a.DisplayName,
                a.ContentType, a.FileSizeBytes, a.LinkedEntityId,
                a.CreatedAt, url));
        }

        return Result.Success<IReadOnlyList<EmployeeAttachmentDto>>(dtos);
    }
}
