using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Employees.Dtos;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Queries;

public sealed record ListEmployeesQuery(
    EmployeeStatus? Status = null,
    bool? IsActive = null,
    LicenceCategory? LicenceCategory = null,
    Guid? StationId = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<ListEmployeesResult>>;

public sealed record ListEmployeesResult(
    IReadOnlyList<EmployeeSummaryDto> Items,
    int Total,
    int Page,
    int PageSize);

public sealed class ListEmployeesQueryHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<ListEmployeesQuery, Result<ListEmployeesResult>>
{
    public async Task<Result<ListEmployeesResult>> Handle(ListEmployeesQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<ListEmployeesResult>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        // isActive=true → Active only; isActive=false → Suspended+Terminated
        var statusFilter = request.Status;
        if (statusFilter is null && request.IsActive.HasValue)
            statusFilter = request.IsActive.Value ? EmployeeStatus.Active : null;

        var (items, total) = await employees.ListAsync(
            orgId, statusFilter, request.IsActive, request.LicenceCategory,
            request.StationId, request.Page, request.PageSize, ct);

        var dtos = items.Select(e => new EmployeeSummaryDto(
            e.Id, e.EmployeeNumber, e.FullName, e.Email, e.Phone,
            e.Status.ToString(), e.IsActive, e.UserId, e.DefaultStationId,
            e.Licences.Count,
            e.ActiveAuthorisations.Count)).ToList();

        return Result.Success(new ListEmployeesResult(dtos, total, request.Page, request.PageSize));
    }
}
