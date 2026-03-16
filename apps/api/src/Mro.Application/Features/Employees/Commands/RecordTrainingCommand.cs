using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Commands;

public sealed class RecordTrainingCommand : IRequest<Result<Unit>>
{
    public required Guid EmployeeId { get; init; }
    public required string CourseCode { get; init; }
    public required string CourseName { get; init; }
    public required string TrainingProvider { get; init; }
    public required TrainingType TrainingType { get; init; }
    public required DateOnly CompletedAt { get; init; }
    public DateOnly? ExpiresAt { get; init; }
    public string? Result { get; init; }
    public string? CertificateRef { get; init; }
}

public sealed class RecordTrainingCommandHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<RecordTrainingCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RecordTrainingCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<Unit>(Error.NotFound("Employee", request.EmployeeId));

        var domainResult = emp.RecordTraining(
            request.CourseCode, request.CourseName, request.TrainingProvider,
            request.TrainingType, request.CompletedAt, currentUser.UserId!.Value,
            request.ExpiresAt, request.Result, request.CertificateRef);

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await employees.UpdateAsync(emp, ct);
        return Result.Success(Unit.Value);
    }
}
