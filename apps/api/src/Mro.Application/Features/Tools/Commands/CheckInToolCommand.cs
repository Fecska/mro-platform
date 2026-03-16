using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Tools.Commands;

public sealed class CheckInToolCommand : IRequest<Result<Unit>>
{
    public required Guid ToolId { get; init; }
}

public sealed class CheckInToolCommandHandler(
    IToolRepository tools,
    ICurrentUserService currentUser)
    : IRequestHandler<CheckInToolCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CheckInToolCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var tool = await tools.GetByIdAsync(request.ToolId, currentUser.OrganisationId.Value, ct);
        if (tool is null)
            return Result.Failure<Unit>(Error.NotFound("Tool", request.ToolId));

        var result = tool.CheckIn(currentUser.UserId!.Value);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await tools.UpdateAsync(tool, ct);
        return Result.Success(Unit.Value);
    }
}
