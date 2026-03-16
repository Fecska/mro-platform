using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.WorkOrders.Queries;

public sealed record BlockerDto(
    Guid Id,
    string BlockerType,
    string Description,
    Guid RaisedByUserId,
    DateTimeOffset RaisedAt,
    bool IsResolved,
    DateTimeOffset? ResolvedAt,
    Guid? ResolvedByUserId,
    string? ResolutionNote);

public sealed record ListBlockersQuery(Guid WorkOrderId, bool ActiveOnly = false) : IRequest<Result<IReadOnlyList<BlockerDto>>>;

public sealed class ListBlockersQueryHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<ListBlockersQuery, Result<IReadOnlyList<BlockerDto>>>
{
    public async Task<Result<IReadOnlyList<BlockerDto>>> Handle(ListBlockersQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<BlockerDto>>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<IReadOnlyList<BlockerDto>>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var blockers = (request.ActiveOnly ? wo.ActiveBlockers : wo.Blockers)
            .Select(b => new BlockerDto(
                b.Id,
                b.BlockerType.ToString(),
                b.Description,
                b.RaisedByUserId,
                b.RaisedAt,
                b.IsResolved,
                b.ResolvedAt,
                b.ResolvedByUserId,
                b.ResolutionNote))
            .ToList();

        return Result.Success<IReadOnlyList<BlockerDto>>(blockers);
    }
}
