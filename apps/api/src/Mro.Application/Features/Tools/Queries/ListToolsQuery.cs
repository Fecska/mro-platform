using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Tools.Dtos;
using Mro.Domain.Aggregates.Tool.Enums;

namespace Mro.Application.Features.Tools.Queries;

public sealed record ListToolsQuery(
    ToolStatus? Status,
    ToolCategory? Category,
    int Page,
    int PageSize) : IRequest<Result<IReadOnlyList<ToolSummaryDto>>>;

public sealed class ListToolsQueryHandler(
    IToolRepository tools,
    ICurrentUserService currentUser)
    : IRequestHandler<ListToolsQuery, Result<IReadOnlyList<ToolSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<ToolSummaryDto>>> Handle(ListToolsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<ToolSummaryDto>>(Error.Forbidden("Organisation context is required."));

        var list = await tools.ListAsync(
            currentUser.OrganisationId.Value, request.Status, request.Category, request.Page, request.PageSize, ct);

        var dtos = list.Select(t => new ToolSummaryDto(
            t.Id, t.ToolNumber, t.Description, t.Category, t.Status,
            t.CalibrationRequired, t.IsCalibrationExpired, t.NextCalibrationDue, t.Location))
            .ToList();

        return Result.Success<IReadOnlyList<ToolSummaryDto>>(dtos);
    }
}
