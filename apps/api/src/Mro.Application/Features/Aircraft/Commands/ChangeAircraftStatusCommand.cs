using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Aircraft.Enums;

namespace Mro.Application.Features.Aircraft.Commands;

public sealed record ChangeAircraftStatusCommand : IRequest<Result>
{
    public required Guid AircraftId { get; init; }
    public required AircraftStatus NewStatus { get; init; }
    public required string Reason { get; init; }
}

public sealed class ChangeAircraftStatusCommandValidator : AbstractValidator<ChangeAircraftStatusCommand>
{
    public ChangeAircraftStatusCommandValidator()
    {
        RuleFor(x => x.AircraftId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class ChangeAircraftStatusCommandHandler(
    IAircraftRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<ChangeAircraftStatusCommand, Result>
{
    public async Task<Result> Handle(ChangeAircraftStatusCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure(Error.Forbidden("Organisation context is required."));

        var ac = await repository.GetByIdAsync(request.AircraftId, currentUser.OrganisationId.Value, ct);
        if (ac is null)
            return Result.Failure(Error.NotFound(nameof(Domain.Aggregates.Aircraft.Aircraft), request.AircraftId));

        var domainResult = ac.ChangeStatus(request.NewStatus, request.Reason, currentUser.UserId!.Value);
        if (domainResult.IsFailure)
            return Result.Failure(Error.InvalidTransition(ac.Status.ToString(), request.NewStatus.ToString()));

        await repository.UpdateAsync(ac, ct);
        return Result.Success();
    }
}
