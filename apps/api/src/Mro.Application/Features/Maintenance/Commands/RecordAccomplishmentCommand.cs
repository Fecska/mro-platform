using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Maintenance.Commands;

public sealed class RecordAccomplishmentCommand : IRequest<Result<Unit>>
{
    public required Guid DueItemId { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required DateTimeOffset AccomplishedAt { get; init; }
    public decimal? AtHours { get; init; }
    public int? AtCycles { get; init; }
}

public sealed class RecordAccomplishmentCommandHandler(
    IDueItemRepository dueItems,
    ICurrentUserService currentUser)
    : IRequestHandler<RecordAccomplishmentCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RecordAccomplishmentCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var item = await dueItems.GetByIdAsync(request.DueItemId, currentUser.OrganisationId.Value, ct);
        if (item is null)
            return Result.Failure<Unit>(Error.NotFound("DueItem", request.DueItemId));

        var result = item.RecordAccomplishment(
            request.AccomplishedAt, request.WorkOrderId, currentUser.UserId!.Value,
            request.AtHours, request.AtCycles);

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await dueItems.UpdateAsync(item, ct);
        return Result.Success(Unit.Value);
    }
}
