using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Commands;

public sealed class AddLicenceCommand : IRequest<Result<Unit>>
{
    public required Guid EmployeeId { get; init; }
    public required string LicenceNumber { get; init; }
    public required LicenceCategory Category { get; init; }
    public required string IssuingAuthority { get; init; }
    public required DateOnly IssuedAt { get; init; }
    public string? Subcategory { get; init; }
    public DateOnly? ExpiresAt { get; init; }
    public string? TypeRatings { get; init; }
    public string? ScopeNotes { get; init; }
    public string? AttachmentRef { get; init; }
}

public sealed class AddLicenceCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<AddLicenceCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AddLicenceCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Unit>(Error.NotFound("Employee", request.EmployeeId));

        var result = emp.AddLicence(
            request.LicenceNumber, request.Category, request.IssuingAuthority,
            request.IssuedAt, currentUser.UserId!.Value,
            request.Subcategory, request.ExpiresAt, request.TypeRatings,
            request.ScopeNotes, request.AttachmentRef);

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await employees.UpdateAsync(emp, ct);
        return Result.Success(Unit.Value);
    }
}
