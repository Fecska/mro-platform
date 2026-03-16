using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Defects.Dtos;
using Mro.Domain.Aggregates.Defect.Enums;

namespace Mro.Application.Features.Defects.Queries;

public sealed record ListDefectsQuery(
    Guid? AircraftId = null,
    DefectStatus? Status = null,
    DefectSeverity? Severity = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<ListDefectsResult>>;

public sealed record ListDefectsResult(
    IReadOnlyList<DefectSummaryDto> Items,
    int Total,
    int Page,
    int PageSize);

public sealed class ListDefectsQueryHandler(
    IDefectRepository defects,
    ICurrentUserService currentUser)
    : IRequestHandler<ListDefectsQuery, Result<ListDefectsResult>>
{
    public async Task<Result<ListDefectsResult>> Handle(ListDefectsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<ListDefectsResult>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;
        var items = await defects.ListAsync(orgId, request.AircraftId, request.Status, request.Severity, request.Page, request.PageSize, ct);
        var total = await defects.CountAsync(orgId, request.AircraftId, request.Status, request.Severity, ct);

        var dtos = items.Select(d => new DefectSummaryDto(
            d.Id,
            d.DefectNumber,
            d.AircraftId,
            d.Status.ToString(),
            d.Severity.ToString(),
            d.Source.ToString(),
            d.AtaChapter,
            d.Description,
            d.DiscoveredAt,
            d.IsAdMandated,
            d.AssignedToUserId,
            d.CreatedAt)).ToList();

        return Result.Success(new ListDefectsResult(dtos, total, request.Page, request.PageSize));
    }
}
