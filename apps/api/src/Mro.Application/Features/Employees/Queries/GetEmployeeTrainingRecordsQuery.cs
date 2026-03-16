using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Employees.Dtos;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Queries;

public sealed record GetEmployeeTrainingRecordsQuery(
    Guid EmployeeId,
    TrainingType? TrainingType = null,
    bool ExpiredOnly = false)
    : IRequest<Result<IReadOnlyList<TrainingRecordDto>>>;

public sealed class GetEmployeeTrainingRecordsQueryHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<GetEmployeeTrainingRecordsQuery, Result<IReadOnlyList<TrainingRecordDto>>>
{
    public async Task<Result<IReadOnlyList<TrainingRecordDto>>> Handle(
        GetEmployeeTrainingRecordsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<TrainingRecordDto>>(
                Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<IReadOnlyList<TrainingRecordDto>>(
                Error.NotFound("Employee", request.EmployeeId));

        var source = emp.TrainingRecords.AsEnumerable();

        if (request.TrainingType.HasValue)
            source = source.Where(t => t.TrainingType == request.TrainingType.Value);

        if (request.ExpiredOnly)
            source = source.Where(t => t.IsExpired);

        var dtos = source
            .OrderByDescending(t => t.CompletedAt)
            .Select(t => new TrainingRecordDto(
                t.Id, t.CourseCode, t.CourseName, t.TrainingProvider,
                t.TrainingType.ToString(), t.CompletedAt, t.ExpiresAt,
                t.Result, t.CertificateRef, t.IsExpired, t.IsRecurring))
            .ToList();

        return Result.Success<IReadOnlyList<TrainingRecordDto>>(dtos);
    }
}
