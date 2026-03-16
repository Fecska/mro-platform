using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Commands;

public sealed class DeleteEmployeeAttachmentCommand : IRequest<Result<Unit>>
{
    public required Guid EmployeeId { get; init; }
    public required Guid AttachmentId { get; init; }
}

public sealed class DeleteEmployeeAttachmentCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteEmployeeAttachmentCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteEmployeeAttachmentCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetWithAttachmentsAsync(
            request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Unit>(Error.NotFound("Employee", request.EmployeeId));

        var result = emp.RemoveAttachment(request.AttachmentId, currentUser.UserId!.Value);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.NotFound("EmployeeAttachment", request.AttachmentId));

        await employees.UpdateAsync(emp, ct);
        return Result.Success(Unit.Value);
    }
}
