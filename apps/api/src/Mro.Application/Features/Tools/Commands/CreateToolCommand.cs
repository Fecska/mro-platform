using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Tool.Enums;

namespace Mro.Application.Features.Tools.Commands;

public sealed class CreateToolCommand : IRequest<Result<Guid>>
{
    public required string ToolNumber { get; init; }
    public required string Description { get; init; }
    public required ToolCategory Category { get; init; }
    public required bool CalibrationRequired { get; init; }
    public string? Location { get; init; }
}

public sealed class CreateToolCommandHandler(
    IToolRepository tools,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateToolCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateToolCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId   = currentUser.OrganisationId.Value;
        var actorId = currentUser.UserId!.Value;

        if (await tools.ExistsAsync(request.ToolNumber, orgId, ct))
            return Result.Failure<Guid>(Error.Conflict($"Tool number '{request.ToolNumber}' already exists."));

        var tool = Mro.Domain.Aggregates.Tool.Tool.Create(
            request.ToolNumber, request.Description, request.Category,
            request.CalibrationRequired, orgId, actorId, request.Location);

        await tools.AddAsync(tool, ct);
        return Result.Success(tool.Id);
    }
}
