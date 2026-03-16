using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Tools.Dtos;

namespace Mro.Application.Features.Tools.Queries;

public sealed record ListCalibrationsQuery(Guid ToolId)
    : IRequest<Result<IReadOnlyList<CalibrationRecordDto>>>;

public sealed class ListCalibrationsQueryHandler(
    IToolRepository tools,
    ICurrentUserService currentUser)
    : IRequestHandler<ListCalibrationsQuery, Result<IReadOnlyList<CalibrationRecordDto>>>
{
    public async Task<Result<IReadOnlyList<CalibrationRecordDto>>> Handle(
        ListCalibrationsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<CalibrationRecordDto>>(
                Error.Forbidden("Organisation context is required."));

        var tool = await tools.GetByIdAsync(request.ToolId, currentUser.OrganisationId.Value, ct);
        if (tool is null)
            return Result.Failure<IReadOnlyList<CalibrationRecordDto>>(
                Error.NotFound("Tool", request.ToolId));

        var dtos = tool.CalibrationRecords
            .OrderByDescending(c => c.CalibratedAt)
            .Select(c => new CalibrationRecordDto(
                c.Id,
                c.CalibratedAt,
                c.ExpiresAt,
                c.PerformedBy,
                c.CertificateRef,
                c.Notes))
            .ToList();

        return Result.Success<IReadOnlyList<CalibrationRecordDto>>(dtos);
    }
}
