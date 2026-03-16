using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Inventory.Dtos;
using Mro.Domain.Aggregates.Inventory.Enums;

namespace Mro.Application.Features.Inventory.Queries;

public sealed record ListPartsQuery(
    PartStatus? Status,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<PartSummaryDto>>>;

public sealed class ListPartsQueryHandler(
    IPartRepository parts,
    ICurrentUserService currentUser)
    : IRequestHandler<ListPartsQuery, Result<IReadOnlyList<PartSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<PartSummaryDto>>> Handle(ListPartsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<PartSummaryDto>>(Error.Forbidden("Organisation context is required."));

        var list = await parts.ListAsync(currentUser.OrganisationId.Value, request.Status, request.Page, request.PageSize, ct);

        var dtos = list.Select(p => new PartSummaryDto(
            p.Id, p.PartNumber, p.Description, p.AtaChapter,
            p.UnitOfMeasure, p.Manufacturer, p.Status))
            .ToList();

        return Result.Success<IReadOnlyList<PartSummaryDto>>(dtos);
    }
}
