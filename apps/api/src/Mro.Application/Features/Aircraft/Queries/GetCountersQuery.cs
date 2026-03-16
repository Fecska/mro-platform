using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Aircraft.Dtos;

namespace Mro.Application.Features.Aircraft.Queries;

public sealed record GetCountersQuery(Guid AircraftId) : IRequest<Result<IReadOnlyList<CounterDto>>>;

public sealed class GetCountersQueryHandler(
    IAircraftRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetCountersQuery, Result<IReadOnlyList<CounterDto>>>
{
    public async Task<Result<IReadOnlyList<CounterDto>>> Handle(
        GetCountersQuery request,
        CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<CounterDto>>(Error.Forbidden("Organisation context is required."));

        var ac = await repository.GetByIdAsync(request.AircraftId, currentUser.OrganisationId.Value, ct);
        if (ac is null)
            return Result.Failure<IReadOnlyList<CounterDto>>(
                Error.NotFound(nameof(Domain.Aggregates.Aircraft.Aircraft), request.AircraftId));

        var dtos = ac.Counters
            .Select(c => new CounterDto(c.CounterType.ToString(), c.Value, c.LastUpdatedAt))
            .ToList();

        return Result.Success<IReadOnlyList<CounterDto>>(dtos);
    }
}
