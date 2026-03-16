using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Tools.Dtos;

namespace Mro.Application.Features.Tools.Queries;

public sealed record GetToolQuery(Guid Id) : IRequest<Result<ToolDetailDto>>;

public sealed class GetToolQueryHandler(
    IToolRepository tools,
    ICurrentUserService currentUser)
    : IRequestHandler<GetToolQuery, Result<ToolDetailDto>>
{
    public async Task<Result<ToolDetailDto>> Handle(GetToolQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<ToolDetailDto>(Error.Forbidden("Organisation context is required."));

        var tool = await tools.GetByIdAsync(request.Id, currentUser.OrganisationId.Value, ct);
        if (tool is null)
            return Result.Failure<ToolDetailDto>(Error.NotFound("Tool", request.Id));

        var calibrations = tool.CalibrationRecords
            .Select(c => new CalibrationRecordDto(c.Id, c.CalibratedAt, c.ExpiresAt, c.PerformedBy, c.CertificateRef, c.Notes))
            .ToList();

        return Result.Success(new ToolDetailDto(
            tool.Id, tool.ToolNumber, tool.Description, tool.Category, tool.Status,
            tool.CalibrationRequired, tool.IsCalibrationExpired, tool.NextCalibrationDue,
            tool.CheckedOutToWorkOrderTaskId, tool.CheckedOutByUserId, tool.CheckedOutAt,
            tool.Location, calibrations));
    }
}
