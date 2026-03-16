using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Inventory.Commands;

public sealed class ReturnMaterialCommand : IRequest<Result<Unit>>
{
    public required Guid StockItemId { get; init; }
    public required decimal Quantity { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required Guid WorkOrderTaskId { get; init; }
    public required string Reason { get; init; }
    public Guid? OriginalIssueId { get; init; }
    public string? BatchNumber { get; init; }
    public string? SerialNumber { get; init; }
}

public sealed class ReturnMaterialCommandValidator : AbstractValidator<ReturnMaterialCommand>
{
    public ReturnMaterialCommandValidator()
    {
        RuleFor(x => x.StockItemId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.WorkOrderId).NotEmpty();
        RuleFor(x => x.WorkOrderTaskId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class ReturnMaterialCommandHandler(
    IStockItemRepository stockItems,
    ICurrentUserService currentUser)
    : IRequestHandler<ReturnMaterialCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ReturnMaterialCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var item = await stockItems.GetByIdAsync(request.StockItemId, currentUser.OrganisationId.Value, ct);
        if (item is null)
            return Result.Failure<Unit>(Error.NotFound("StockItem", request.StockItemId));

        var actorId = currentUser.UserId!.Value;
        var domainResult = item.Return(
            request.Quantity,
            request.WorkOrderId,
            request.WorkOrderTaskId,
            actorId,
            request.Reason,
            actorId,
            request.OriginalIssueId,
            request.BatchNumber,
            request.SerialNumber);

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await stockItems.UpdateAsync(item, ct);
        return Result.Success(Unit.Value);
    }
}
