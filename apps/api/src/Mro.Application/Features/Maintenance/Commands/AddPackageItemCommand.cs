using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Maintenance.Commands;

public sealed class AddPackageItemCommand : IRequest<Result<Unit>>
{
    public required Guid WorkPackageId { get; init; }
    public required string Description { get; init; }
    public Guid? DueItemId { get; init; }
    public string? TaskReference { get; init; }
    public decimal? EstimatedManHours { get; init; }
}

public sealed class AddPackageItemCommandHandler(
    IWorkPackageRepository packages,
    ICurrentUserService currentUser)
    : IRequestHandler<AddPackageItemCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AddPackageItemCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wp = await packages.GetByIdAsync(request.WorkPackageId, currentUser.OrganisationId.Value, ct);
        if (wp is null)
            return Result.Failure<Unit>(Error.NotFound("WorkPackage", request.WorkPackageId));

        var result = wp.AddItem(
            request.Description, currentUser.UserId!.Value,
            request.DueItemId, request.TaskReference, request.EstimatedManHours);

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await packages.UpdateAsync(wp, ct);
        return Result.Success(Unit.Value);
    }
}
