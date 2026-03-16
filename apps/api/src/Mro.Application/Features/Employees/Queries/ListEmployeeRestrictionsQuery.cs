using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Queries;

public sealed class ListEmployeeRestrictionsQuery : IRequest<Result<IReadOnlyList<EmployeeRestrictionDto>>>
{
    public required Guid EmployeeId { get; init; }
    public bool ActiveOnly { get; init; }
}

public sealed record EmployeeRestrictionDto(
    Guid Id,
    RestrictionType RestrictionType,
    string? Details,
    Guid? StationId,
    Guid RaisedByUserId,
    DateOnly ActiveFrom,
    DateOnly? ActiveUntil,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed class ListEmployeeRestrictionsQueryHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<ListEmployeeRestrictionsQuery, Result<IReadOnlyList<EmployeeRestrictionDto>>>
{
    public async Task<Result<IReadOnlyList<EmployeeRestrictionDto>>> Handle(
        ListEmployeeRestrictionsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<EmployeeRestrictionDto>>(
                Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        var emp = await employees.GetByIdAsync(request.EmployeeId, orgId, ct);
        if (emp is null)
            return Result.Failure<IReadOnlyList<EmployeeRestrictionDto>>(
                Error.NotFound("Employee", request.EmployeeId));

        var restrictions = await employees.ListRestrictionsAsync(
            request.EmployeeId, orgId, request.ActiveOnly, ct);

        var dtos = restrictions.Select(r => new EmployeeRestrictionDto(
            r.Id, r.RestrictionType, r.Details, r.StationId,
            r.RaisedByUserId, r.ActiveFrom, r.ActiveUntil, r.IsActive, r.CreatedAt))
            .ToList();

        return Result.Success<IReadOnlyList<EmployeeRestrictionDto>>(dtos);
    }
}
