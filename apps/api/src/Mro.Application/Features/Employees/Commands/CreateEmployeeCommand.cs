using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee;

namespace Mro.Application.Features.Employees.Commands;

public sealed class CreateEmployeeCommand : IRequest<Result<Guid>>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public Guid? UserId { get; init; }
    public string? Phone { get; init; }
    public string? NationalityCode { get; init; }
    public Guid? DefaultStationId { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
}

public sealed class CreateEmployeeCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateEmployeeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;
        var actorId = currentUser.UserId!.Value;

        var count = await employees.CountAsync(orgId, ct);
        var empNumber = $"EMP-{(count + 1):D4}";

        var emp = Employee.Create(
            empNumber,
            request.FirstName,
            request.LastName,
            request.Email,
            request.DateOfBirth,
            orgId,
            actorId,
            request.UserId,
            request.Phone,
            request.NationalityCode,
            request.DefaultStationId);

        await employees.AddAsync(emp, ct);
        return Result.Success(emp.Id);
    }
}
