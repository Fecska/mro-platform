using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Aircraft.Commands;

public sealed record RemoveComponentCommand : IRequest<Result>
{
    public required Guid AircraftId { get; init; }
    public required Guid ComponentId { get; init; }
    public required string Reason { get; init; }
    public Guid? WorkOrderId { get; init; }
}

public sealed class RemoveComponentCommandValidator : AbstractValidator<RemoveComponentCommand>
{
    public RemoveComponentCommandValidator()
    {
        RuleFor(x => x.AircraftId).NotEmpty();
        RuleFor(x => x.ComponentId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class RemoveComponentCommandHandler(
    IAircraftRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<RemoveComponentCommand, Result>
{
    public async Task<Result> Handle(RemoveComponentCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure(Error.Forbidden("Organisation context is required."));

        var ac = await repository.GetByIdAsync(request.AircraftId, currentUser.OrganisationId.Value, ct);
        if (ac is null)
            return Result.Failure(Error.NotFound(nameof(Domain.Aggregates.Aircraft.Aircraft), request.AircraftId));

        var actorId = currentUser.UserId!.Value;
        var domainResult = ac.RemoveComponent(request.ComponentId, request.Reason, actorId, actorId, request.WorkOrderId);

        if (domainResult.IsFailure)
            return Result.Failure(Error.NotFound("InstalledComponent", request.ComponentId));

        await repository.UpdateAsync(ac, ct);
        return Result.Success();
    }
}
