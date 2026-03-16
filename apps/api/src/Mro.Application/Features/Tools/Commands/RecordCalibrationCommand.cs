using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Tools.Commands;

public sealed class RecordCalibrationCommand : IRequest<Result<Unit>>
{
    public required Guid ToolId { get; init; }
    public required DateTimeOffset CalibratedAt { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required string PerformedBy { get; init; }
    public string? CertificateRef { get; init; }
    public string? Notes { get; init; }
}

public sealed class RecordCalibrationCommandHandler(
    IToolRepository tools,
    ICurrentUserService currentUser)
    : IRequestHandler<RecordCalibrationCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RecordCalibrationCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var tool = await tools.GetByIdAsync(request.ToolId, currentUser.OrganisationId.Value, ct);
        if (tool is null)
            return Result.Failure<Unit>(Error.NotFound("Tool", request.ToolId));

        var result = tool.RecordCalibration(
            request.CalibratedAt, request.ExpiresAt, request.PerformedBy,
            currentUser.UserId!.Value, request.CertificateRef, request.Notes);

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await tools.UpdateAsync(tool, ct);
        return Result.Success(Unit.Value);
    }
}
