using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Commands;

public sealed class GrantAuthorisationCommand : IRequest<Result<Unit>>
{
    public required Guid EmployeeId { get; init; }
    public required string AuthorisationNumber { get; init; }
    public required LicenceCategory Category { get; init; }
    public required string Scope { get; init; }
    public required Guid IssuingLicenceId { get; init; }
    public required Guid IssuedByUserId { get; init; }
    public required DateOnly ValidFrom { get; init; }
    public string? AircraftTypes { get; init; }
    public DateOnly? ValidUntil { get; init; }
    public string? ComponentScope { get; init; }
    public string? StationScope { get; init; }
    public string? IssuingAuthority { get; init; }
}

public sealed class GrantAuthorisationCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<GrantAuthorisationCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(GrantAuthorisationCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Unit>(Error.NotFound("Employee", request.EmployeeId));

        var result = emp.GrantAuthorisation(
            request.AuthorisationNumber, request.Category, request.Scope,
            request.IssuingLicenceId, request.IssuedByUserId, request.ValidFrom,
            currentUser.UserId!.Value, request.AircraftTypes, request.ValidUntil,
            request.ComponentScope, request.StationScope, request.IssuingAuthority);

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await employees.UpdateAsync(emp, ct);
        return Result.Success(Unit.Value);
    }
}
