using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Inventory;

namespace Mro.Application.Features.Inventory.Commands;

public sealed class CreateBinLocationCommand : IRequest<Result<Guid>>
{
    public required string Code { get; init; }
    public string? Description { get; init; }
    public string? StoreRoom { get; init; }
}

public sealed class CreateBinLocationCommandHandler(
    IBinLocationRepository binLocations,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateBinLocationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBinLocationCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var loc = BinLocation.Create(
            request.Code, currentUser.OrganisationId.Value, currentUser.UserId!.Value,
            request.Description, request.StoreRoom);

        await binLocations.AddAsync(loc, ct);
        return Result.Success(loc.Id);
    }
}
