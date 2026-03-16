using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Employees.Dtos;

namespace Mro.Application.Features.Employees.Queries;

public sealed record GetEmployeeAuthorisationsQuery(
    Guid EmployeeId,
    bool ActiveOnly = false)
    : IRequest<Result<IReadOnlyList<AuthorisationDto>>>;

public sealed class GetEmployeeAuthorisationsQueryHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<GetEmployeeAuthorisationsQuery, Result<IReadOnlyList<AuthorisationDto>>>
{
    public async Task<Result<IReadOnlyList<AuthorisationDto>>> Handle(
        GetEmployeeAuthorisationsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<AuthorisationDto>>(
                Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<IReadOnlyList<AuthorisationDto>>(
                Error.NotFound("Employee", request.EmployeeId));

        var source = request.ActiveOnly
            ? emp.Authorisations.Where(a => a.IsCurrent)
            : emp.Authorisations.AsEnumerable();

        var dtos = source
            .OrderByDescending(a => a.ValidFrom)
            .Select(a => new AuthorisationDto(
                a.Id, a.AuthorisationNumber, a.Category.ToString(), a.Scope,
                a.AircraftTypes, a.ComponentScope, a.StationScope, a.IssuingAuthority,
                a.ValidFrom, a.ValidUntil, a.IssuedByUserId,
                a.Status.ToString(), a.RevisionNumber, a.SuspensionReason,
                a.IsExpired, a.IsCurrent))
            .ToList();

        return Result.Success<IReadOnlyList<AuthorisationDto>>(dtos);
    }
}
