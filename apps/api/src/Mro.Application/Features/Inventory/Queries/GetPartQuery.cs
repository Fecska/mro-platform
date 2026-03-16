using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Inventory.Dtos;

namespace Mro.Application.Features.Inventory.Queries;

public sealed record GetPartQuery(Guid Id) : IRequest<Result<PartDetailDto>>;

public sealed class GetPartQueryHandler(
    IPartRepository parts,
    ICurrentUserService currentUser)
    : IRequestHandler<GetPartQuery, Result<PartDetailDto>>
{
    public async Task<Result<PartDetailDto>> Handle(GetPartQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<PartDetailDto>(Error.Forbidden("Organisation context is required."));

        var part = await parts.GetByIdAsync(request.Id, currentUser.OrganisationId.Value, ct);
        if (part is null)
            return Result.Failure<PartDetailDto>(Error.NotFound("Part", request.Id));

        return Result.Success(new PartDetailDto(
            part.Id, part.PartNumber, part.Description, part.AtaChapter,
            part.UnitOfMeasure, part.Manufacturer, part.ManufacturerPartNumber,
            part.TraceabilityRequired, part.MinStockLevel, part.Status));
    }
}
