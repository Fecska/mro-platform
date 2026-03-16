using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Queries;

public sealed class ListEmployeeNotesQuery : IRequest<Result<IReadOnlyList<EmployeeNoteDto>>>
{
    public required Guid EmployeeId { get; init; }
}

public sealed record EmployeeNoteDto(
    Guid Id,
    string NoteText,
    bool IsConfidential,
    Guid CreatedBy,
    DateTimeOffset CreatedAt);

public sealed class ListEmployeeNotesQueryHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<ListEmployeeNotesQuery, Result<IReadOnlyList<EmployeeNoteDto>>>
{
    public async Task<Result<IReadOnlyList<EmployeeNoteDto>>> Handle(
        ListEmployeeNotesQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<EmployeeNoteDto>>(
                Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        var emp = await employees.GetByIdAsync(request.EmployeeId, orgId, ct);
        if (emp is null)
            return Result.Failure<IReadOnlyList<EmployeeNoteDto>>(
                Error.NotFound("Employee", request.EmployeeId));

        var notes = await employees.ListNotesAsync(request.EmployeeId, orgId, ct);

        var dtos = notes.Select(n => new EmployeeNoteDto(
            n.Id, n.NoteText, n.IsConfidential, n.CreatedBy, n.CreatedAt))
            .ToList();

        return Result.Success<IReadOnlyList<EmployeeNoteDto>>(dtos);
    }
}
