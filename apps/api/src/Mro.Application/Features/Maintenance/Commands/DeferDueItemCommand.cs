using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Maintenance.Commands;

public sealed class DeferDueItemCommand : IRequest<Result<Unit>>
{
    public required Guid DueItemId { get; init; }
    public required DateTimeOffset NewDueDate { get; init; }
    public required string Justification { get; init; }
}

public sealed class DeferDueItemCommandHandler(
    IDueItemRepository dueItems,
    ICurrentUserService currentUser)
    : IRequestHandler<DeferDueItemCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeferDueItemCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var item = await dueItems.GetByIdAsync(request.DueItemId, currentUser.OrganisationId.Value, ct);
        if (item is null)
            return Result.Failure<Unit>(Error.NotFound("DueItem", request.DueItemId));

        var result = item.Defer(request.NewDueDate, request.Justification, currentUser.UserId!.Value);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await dueItems.UpdateAsync(item, ct);
        return Result.Success(Unit.Value);
    }
}
