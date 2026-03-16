using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Aircraft.Dtos;

namespace Mro.Application.Features.Aircraft.Queries;

public sealed record GetStatusHistoryQuery(Guid AircraftId) : IRequest<Result<IReadOnlyList<StatusHistoryDto>>>;

public sealed class GetStatusHistoryQueryHandler(
    IAircraftRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetStatusHistoryQuery, Result<IReadOnlyList<StatusHistoryDto>>>
{
    public async Task<Result<IReadOnlyList<StatusHistoryDto>>> Handle(
        GetStatusHistoryQuery request,
        CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<StatusHistoryDto>>(Error.Forbidden("Organisation context is required."));

        var ac = await repository.GetByIdAsync(request.AircraftId, currentUser.OrganisationId.Value, ct);
        if (ac is null)
            return Result.Failure<IReadOnlyList<StatusHistoryDto>>(
                Error.NotFound(nameof(Domain.Aggregates.Aircraft.Aircraft), request.AircraftId));

        var dtos = ac.StatusHistory
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new StatusHistoryDto(
                h.FromStatus.ToString(),
                h.ToStatus.ToString(),
                h.Reason,
                h.ActorId,
                h.ChangedAt))
            .ToList();

        return Result.Success<IReadOnlyList<StatusHistoryDto>>(dtos);
    }
}
