using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Aircraft.Dtos;

namespace Mro.Application.Features.Aircraft.Queries;

public sealed record ListAircraftQuery(int Page = 1, int PageSize = 50) : IRequest<Result<ListAircraftResult>>;

public sealed record ListAircraftResult(
    IReadOnlyList<AircraftSummaryDto> Items,
    int Total,
    int Page,
    int PageSize);

public sealed class ListAircraftQueryHandler(
    IAircraftRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<ListAircraftQuery, Result<ListAircraftResult>>
{
    public async Task<Result<ListAircraftResult>> Handle(ListAircraftQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<ListAircraftResult>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;
        var items = await repository.ListAsync(orgId, request.Page, request.PageSize, ct);
        var total = await repository.CountAsync(orgId, ct);

        var dtos = items.Select(ac => new AircraftSummaryDto(
            ac.Id,
            ac.Registration,
            ac.SerialNumber,
            ac.AircraftType?.IcaoTypeCode ?? string.Empty,
            ac.AircraftType?.Manufacturer ?? string.Empty,
            ac.AircraftType?.Model ?? string.Empty,
            ac.Status.ToString(),
            ac.ManufactureDate,
            ac.Remarks)).ToList();

        return Result.Success(new ListAircraftResult(dtos, total, request.Page, request.PageSize));
    }
}
