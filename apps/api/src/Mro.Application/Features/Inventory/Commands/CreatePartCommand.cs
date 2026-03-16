using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Inventory;

namespace Mro.Application.Features.Inventory.Commands;

public sealed class CreatePartCommand : IRequest<Result<Guid>>
{
    public required string PartNumber { get; init; }
    public required string Description { get; init; }
    public required string UnitOfMeasure { get; init; }
    public string? AtaChapter { get; init; }
    public string? Manufacturer { get; init; }
    public string? ManufacturerPartNumber { get; init; }
    public bool TraceabilityRequired { get; init; }
    public decimal MinStockLevel { get; init; }
}

public sealed class CreatePartCommandHandler(
    IPartRepository parts,
    ICurrentUserService currentUser)
    : IRequestHandler<CreatePartCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreatePartCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId   = currentUser.OrganisationId.Value;
        var actorId = currentUser.UserId!.Value;

        if (await parts.ExistsAsync(request.PartNumber, orgId, ct))
            return Result.Failure<Guid>(Error.Conflict($"Part number '{request.PartNumber}' already exists."));

        var part = Part.Create(
            request.PartNumber, request.Description, request.UnitOfMeasure,
            orgId, actorId,
            request.AtaChapter, request.Manufacturer, request.ManufacturerPartNumber,
            request.TraceabilityRequired, request.MinStockLevel);

        await parts.AddAsync(part, ct);
        return Result.Success(part.Id);
    }
}
