using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Maintenance;
using Mro.Domain.Aggregates.Maintenance.Enums;

namespace Mro.Application.Features.Maintenance.Commands;

public sealed class CreateDueItemCommand : IRequest<Result<Guid>>
{
    public required string DueItemRef { get; init; }
    public required Guid AircraftId { get; init; }
    public required DueItemType DueItemType { get; init; }
    public required IntervalType IntervalType { get; init; }
    public required string Description { get; init; }
    public Guid? MaintenanceProgramId { get; init; }
    public string? RegulatoryRef { get; init; }
    public decimal? IntervalValue { get; init; }
    public int? IntervalDays { get; init; }
    public decimal? ToleranceValue { get; init; }
    public DateTimeOffset? NextDueDate { get; init; }
    public decimal? NextDueHours { get; init; }
    public int? NextDueCycles { get; init; }
}

public sealed class CreateDueItemCommandHandler(
    IDueItemRepository dueItems,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateDueItemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateDueItemCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var item = DueItem.Create(
            request.DueItemRef, request.AircraftId, request.DueItemType, request.IntervalType,
            request.Description, currentUser.OrganisationId.Value, currentUser.UserId!.Value,
            request.MaintenanceProgramId, request.RegulatoryRef,
            request.IntervalValue, request.IntervalDays, request.ToleranceValue,
            request.NextDueDate, request.NextDueHours, request.NextDueCycles);

        await dueItems.AddAsync(item, ct);
        return Result.Success(item.Id);
    }
}
