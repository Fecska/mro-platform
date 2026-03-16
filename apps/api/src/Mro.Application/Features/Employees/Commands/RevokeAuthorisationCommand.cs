using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Commands;

public sealed class RevokeAuthorisationCommand : IRequest<Result<Unit>>
{
    public required Guid EmployeeId { get; init; }
    public required Guid AuthorisationId { get; init; }
    public required string Reason { get; init; }
}

public sealed class RevokeAuthorisationCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<RevokeAuthorisationCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RevokeAuthorisationCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Unit>(Error.NotFound("Employee", request.EmployeeId));

        var actorId = currentUser.UserId!.Value;

        var result = emp.RevokeAuthorisation(request.AuthorisationId, request.Reason, actorId, actorId);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await employees.UpdateAsync(emp, ct);
        return Result.Success(Unit.Value);
    }
}
