using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.WorkOrders.Commands;

public sealed class UpdateWorkOrderCommand : IRequest<Result<Unit>>
{
    public required Guid WorkOrderId { get; init; }
    public string? Title { get; init; }
    public Guid? StationId { get; init; }
    /// <summary>Set to true to clear the station (StationId = null).</summary>
    public bool ClearStation { get; init; }
    public DateTimeOffset? PlannedStartAt { get; init; }
    public DateTimeOffset? PlannedEndAt { get; init; }
    public string? CustomerOrderRef { get; init; }
}

public sealed class UpdateWorkOrderCommandValidator : AbstractValidator<UpdateWorkOrderCommand>
{
    public UpdateWorkOrderCommandValidator()
    {
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300).When(x => x.Title is not null);
        RuleFor(x => x.CustomerOrderRef).MaximumLength(100).When(x => x.CustomerOrderRef is not null);
    }
}

public sealed class UpdateWorkOrderCommandHandler(
    IWorkOrderRepository workOrders,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateWorkOrderCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateWorkOrderCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, currentUser.OrganisationId.Value, ct);
        if (wo is null)
            return Result.Failure<Unit>(Error.NotFound("WorkOrder", request.WorkOrderId));

        var domainResult = wo.UpdateDetails(
            request.Title,
            request.StationId,
            request.ClearStation,
            request.PlannedStartAt,
            request.PlannedEndAt,
            request.CustomerOrderRef,
            currentUser.UserId!.Value);

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await workOrders.UpdateAsync(wo, ct);
        return Result.Success(Unit.Value);
    }
}
