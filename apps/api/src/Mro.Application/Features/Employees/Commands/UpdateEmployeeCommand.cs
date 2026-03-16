using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Commands;

public sealed class UpdateEmployeeCommand : IRequest<Result<Unit>>
{
    public required Guid EmployeeId { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public Guid? DefaultStationId { get; init; }
    public bool ClearDefaultStation { get; init; }
    public string? NationalityCode { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
}

public sealed class UpdateEmployeeCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateEmployeeCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateEmployeeCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Unit>(Error.NotFound("Employee", request.EmployeeId));

        var domainResult = emp.UpdateDetails(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.DefaultStationId,
            request.ClearDefaultStation,
            request.NationalityCode,
            request.EmergencyContactName,
            request.EmergencyContactPhone,
            currentUser.UserId!.Value);

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await employees.UpdateAsync(emp, ct);
        return Result.Success(Unit.Value);
    }
}
