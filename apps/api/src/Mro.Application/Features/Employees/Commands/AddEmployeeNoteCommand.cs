using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Commands;

public sealed class AddEmployeeNoteCommand : IRequest<Result<Guid>>
{
    public required Guid EmployeeId { get; init; }
    public required string NoteText { get; init; }
    public bool IsConfidential { get; init; }
}

public sealed class AddEmployeeNoteCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<AddEmployeeNoteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddEmployeeNoteCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetWithNotesAsync(
            request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Guid>(Error.NotFound("Employee", request.EmployeeId));

        var result = emp.AddNote(request.NoteText, request.IsConfidential, currentUser.UserId!.Value);
        if (result.IsFailure)
            return Result.Failure<Guid>(Error.Validation(result.ErrorMessage!));

        await employees.UpdateAsync(emp, ct);

        var note = emp.Notes.Last();
        return Result.Success(note.Id);
    }
}
