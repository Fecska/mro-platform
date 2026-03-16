using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Commands;

public sealed class AddEmployeeRestrictionCommand : IRequest<Result<Guid>>
{
    public required Guid EmployeeId { get; init; }
    public required RestrictionType RestrictionType { get; init; }
    public required Guid RaisedByUserId { get; init; }
    public required DateOnly ActiveFrom { get; init; }
    public string? Details { get; init; }
    public Guid? StationId { get; init; }
    public DateOnly? ActiveUntil { get; init; }
}

public sealed class AddEmployeeRestrictionCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<AddEmployeeRestrictionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddEmployeeRestrictionCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetWithRestrictionsAsync(
            request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Guid>(Error.NotFound("Employee", request.EmployeeId));

        var result = emp.AddRestriction(
            request.RestrictionType,
            request.RaisedByUserId,
            request.ActiveFrom,
            currentUser.UserId!.Value,
            request.Details,
            request.StationId,
            request.ActiveUntil);

        if (result.IsFailure)
            return Result.Failure<Guid>(Error.Validation(result.ErrorMessage!));

        await employees.UpdateAsync(emp, ct);

        var restriction = emp.Restrictions.Last();
        return Result.Success(restriction.Id);
    }
}
