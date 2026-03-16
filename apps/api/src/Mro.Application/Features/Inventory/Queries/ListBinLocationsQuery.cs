using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Inventory.Dtos;

namespace Mro.Application.Features.Inventory.Queries;

public sealed record ListBinLocationsQuery : IRequest<Result<IReadOnlyList<BinLocationDto>>>;

public sealed class ListBinLocationsQueryHandler(
    IBinLocationRepository binLocations,
    ICurrentUserService currentUser)
    : IRequestHandler<ListBinLocationsQuery, Result<IReadOnlyList<BinLocationDto>>>
{
    public async Task<Result<IReadOnlyList<BinLocationDto>>> Handle(ListBinLocationsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<BinLocationDto>>(Error.Forbidden("Organisation context is required."));

        var list = await binLocations.ListAsync(currentUser.OrganisationId.Value, ct);
        var dtos = list.Select(b => new BinLocationDto(b.Id, b.Code, b.Description, b.StoreRoom, b.IsActive))
                       .ToList();
        return Result.Success<IReadOnlyList<BinLocationDto>>(dtos);
    }
}
