using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Tools.Commands;

public sealed class CheckOutToolCommand : IRequest<Result<Unit>>
{
    public required Guid ToolId { get; init; }
    public required Guid WorkOrderTaskId { get; init; }
    public required Guid CheckedOutByUserId { get; init; }
}

public sealed class CheckOutToolCommandHandler(
    IToolRepository tools,
    ICurrentUserService currentUser)
    : IRequestHandler<CheckOutToolCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CheckOutToolCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var tool = await tools.GetByIdAsync(request.ToolId, currentUser.OrganisationId.Value, ct);
        if (tool is null)
            return Result.Failure<Unit>(Error.NotFound("Tool", request.ToolId));

        var result = tool.CheckOut(request.WorkOrderTaskId, request.CheckedOutByUserId, currentUser.UserId!.Value);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await tools.UpdateAsync(tool, ct);
        return Result.Success(Unit.Value);
    }
}
