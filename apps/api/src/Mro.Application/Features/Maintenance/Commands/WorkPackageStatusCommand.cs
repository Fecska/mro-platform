using Mro.Domain.Application;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Maintenance.Enums;

namespace Mro.Application.Features.Maintenance.Commands;

public sealed class ChangeWorkPackageStatusCommand : IRequest<Result<Unit>>
{
    public required Guid WorkPackageId { get; init; }
    public required WorkPackageStatus NewStatus { get; init; }
}

public sealed class ChangeWorkPackageStatusCommandHandler(
    IWorkPackageRepository packages,
    ICurrentUserService currentUser)
    : IRequestHandler<ChangeWorkPackageStatusCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ChangeWorkPackageStatusCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wp = await packages.GetByIdAsync(request.WorkPackageId, currentUser.OrganisationId.Value, ct);
        if (wp is null)
            return Result.Failure<Unit>(Error.NotFound("WorkPackage", request.WorkPackageId));

        var actorId = currentUser.UserId!.Value;

        var result = request.NewStatus switch
        {
            WorkPackageStatus.Released  => wp.Release(actorId),
            WorkPackageStatus.InProgress => wp.Start(actorId),
            WorkPackageStatus.Completed  => wp.Complete(actorId),
            WorkPackageStatus.Closed     => wp.Close(actorId),
            WorkPackageStatus.Cancelled  => wp.Cancel(actorId),
            _ => DomainResult.Failure($"Unsupported status transition to {request.NewStatus}."),
        };

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await packages.UpdateAsync(wp, ct);
        return Result.Success(Unit.Value);
    }
}
