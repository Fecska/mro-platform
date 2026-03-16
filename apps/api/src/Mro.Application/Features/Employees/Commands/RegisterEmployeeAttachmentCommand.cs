using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Commands;

/// <summary>
/// Registers an employee attachment after the client has uploaded the file directly to blob storage.
/// Call <see cref="GetEmployeeAttachmentUploadUrlQuery"/> first to obtain the upload URL and storage path.
/// </summary>
public sealed class RegisterEmployeeAttachmentCommand : IRequest<Result<Guid>>
{
    public required Guid EmployeeId { get; init; }
    public required AttachmentType AttachmentType { get; init; }
    public required string DisplayName { get; init; }
    public required string StoragePath { get; init; }
    public required long FileSizeBytes { get; init; }
    public required string ContentType { get; init; }
    public Guid? LinkedEntityId { get; init; }
}

public sealed class RegisterEmployeeAttachmentCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<RegisterEmployeeAttachmentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterEmployeeAttachmentCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Guid>(Error.NotFound("Employee", request.EmployeeId));

        var result = emp.AddAttachment(
            request.AttachmentType,
            request.DisplayName,
            request.StoragePath,
            request.FileSizeBytes,
            request.ContentType,
            currentUser.UserId!.Value,
            request.LinkedEntityId);

        if (result.IsFailure)
            return Result.Failure<Guid>(Error.Validation(result.ErrorMessage!));

        await employees.UpdateAsync(emp, ct);

        var attachment = emp.Attachments.Last();
        return Result.Success(attachment.Id);
    }
}
