using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Commands;

public sealed class DeleteEmployeeNoteCommand : IRequest<Result<Unit>>
{
    public required Guid EmployeeId { get; init; }
    public required Guid NoteId { get; init; }
}

public sealed class DeleteEmployeeNoteCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<DeleteEmployeeNoteCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteEmployeeNoteCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetWithNotesAsync(
            request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Unit>(Error.NotFound("Employee", request.EmployeeId));

        var result = emp.DeleteNote(request.NoteId, currentUser.UserId!.Value);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.NotFound("EmployeeNote", request.NoteId));

        await employees.UpdateAsync(emp, ct);
        return Result.Success(Unit.Value);
    }
}
