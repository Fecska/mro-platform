using Mro.Domain.Application;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Maintenance.Commands;

public enum PackageItemAction { Accomplish, Defer, NotApplicable }

public sealed class PackageItemActionCommand : IRequest<Result<Unit>>
{
    public required Guid WorkPackageId { get; init; }
    public required Guid ItemId { get; init; }
    public required PackageItemAction Action { get; init; }
    public Guid? WorkOrderId { get; init; }
    public decimal? ActualManHours { get; init; }
    public string? Reason { get; init; }
}

public sealed class PackageItemActionCommandHandler(
    IWorkPackageRepository packages,
    ICurrentUserService currentUser)
    : IRequestHandler<PackageItemActionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(PackageItemActionCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wp = await packages.GetByIdAsync(request.WorkPackageId, currentUser.OrganisationId.Value, ct);
        if (wp is null)
            return Result.Failure<Unit>(Error.NotFound("WorkPackage", request.WorkPackageId));

        var actorId = currentUser.UserId!.Value;

        var result = request.Action switch
        {
            PackageItemAction.Accomplish => wp.AccomplishItem(
                request.ItemId, request.WorkOrderId ?? Guid.Empty, actorId, request.ActualManHours),
            PackageItemAction.Defer => wp.DeferItem(
                request.ItemId, request.Reason ?? string.Empty, actorId),
            PackageItemAction.NotApplicable => wp.SetItemNotApplicable(
                request.ItemId, request.Reason ?? string.Empty, actorId),
            _ => DomainResult.Failure("Unknown action."),
        };

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await packages.UpdateAsync(wp, ct);
        return Result.Success(Unit.Value);
    }
}
