using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Commands;

public sealed class SetEmployeeStatusCommand : IRequest<Result<Unit>>
{
    public required Guid EmployeeId { get; init; }
    public required EmployeeStatus NewStatus { get; init; }
}

public sealed class SetEmployeeStatusCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<SetEmployeeStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SetEmployeeStatusCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Unit>(Error.NotFound("Employee", request.EmployeeId));

        var result = emp.SetStatus(request.NewStatus, currentUser.UserId!.Value);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await employees.UpdateAsync(emp, ct);
        return Result.Success(Unit.Value);
    }
}
