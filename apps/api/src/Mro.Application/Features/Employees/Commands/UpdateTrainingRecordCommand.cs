using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Commands;

public sealed class UpdateTrainingRecordCommand : IRequest<Result<Unit>>
{
    public required Guid TrainingRecordId { get; init; }
    public DateOnly? ExpiresAt { get; init; }
    public bool ClearExpiresAt { get; init; }
    public string? Result { get; init; }
    public string? CertificateRef { get; init; }
}

public sealed class UpdateTrainingRecordCommandHandler(
    ITrainingRecordRepository trainingRecords,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateTrainingRecordCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateTrainingRecordCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var record = await trainingRecords.GetByIdAsync(
            request.TrainingRecordId, currentUser.OrganisationId.Value, ct);
        if (record is null)
            return Result.Failure<Unit>(Error.NotFound("TrainingRecord", request.TrainingRecordId));

        var domainResult = record.Update(
            request.ExpiresAt, request.ClearExpiresAt,
            request.Result, request.CertificateRef,
            currentUser.UserId!.Value);

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await trainingRecords.UpdateAsync(record, ct);
        return Result.Success(Unit.Value);
    }
}
