using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Employees.Dtos;

namespace Mro.Application.Features.Employees.Queries;

public sealed record GetEmployeeLicencesQuery(Guid EmployeeId)
    : IRequest<Result<IReadOnlyList<LicenceDto>>>;

public sealed class GetEmployeeLicencesQueryHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<GetEmployeeLicencesQuery, Result<IReadOnlyList<LicenceDto>>>
{
    public async Task<Result<IReadOnlyList<LicenceDto>>> Handle(
        GetEmployeeLicencesQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<LicenceDto>>(
                Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<IReadOnlyList<LicenceDto>>(
                Error.NotFound("Employee", request.EmployeeId));

        var dtos = emp.Licences
            .OrderByDescending(l => l.IssuedAt)
            .Select(l => new LicenceDto(
                l.Id, l.LicenceNumber, l.Category.ToString(), l.Subcategory,
                l.IssuingAuthority, l.IssuedAt, l.ExpiresAt, l.TypeRatings,
                l.ScopeNotes, l.AttachmentRef, l.IsExpired, l.IsCurrent))
            .ToList();

        return Result.Success<IReadOnlyList<LicenceDto>>(dtos);
    }
}
