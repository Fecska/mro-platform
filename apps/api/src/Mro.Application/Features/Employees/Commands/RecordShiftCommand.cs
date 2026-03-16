using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Commands;

public sealed class RecordShiftCommand : IRequest<Result<Unit>>
{
    public required Guid EmployeeId { get; init; }
    public required DateOnly ShiftDate { get; init; }
    public required TimeOnly StartTime { get; init; }
    public required TimeOnly EndTime { get; init; }
    public required ShiftType ShiftType { get; init; }
    public AvailabilityStatus AvailabilityStatus { get; init; } = AvailabilityStatus.Available;
    public bool IsActual { get; init; }
    public Guid? StationId { get; init; }
    public string? Notes { get; init; }
}

public sealed class RecordShiftCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<RecordShiftCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RecordShiftCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Unit>(Error.NotFound("Employee", request.EmployeeId));

        var result = emp.RecordShift(
            request.ShiftDate, request.StartTime, request.EndTime,
            request.ShiftType, currentUser.UserId!.Value,
            request.AvailabilityStatus, request.IsActual,
            request.StationId, request.Notes);

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await employees.UpdateAsync(emp, ct);
        return Result.Success(Unit.Value);
    }
}
