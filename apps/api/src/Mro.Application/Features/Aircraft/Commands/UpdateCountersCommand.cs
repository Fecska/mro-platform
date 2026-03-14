using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Aircraft.Enums;

namespace Mro.Application.Features.Aircraft.Commands;

public sealed record CounterUpdate(CounterType CounterType, decimal Value);

public sealed record UpdateCountersCommand : IRequest<Result>
{
    public required Guid AircraftId { get; init; }
    public required IReadOnlyList<CounterUpdate> Updates { get; init; }
}

public sealed class UpdateCountersCommandValidator : AbstractValidator<UpdateCountersCommand>
{
    public UpdateCountersCommandValidator()
    {
        RuleFor(x => x.AircraftId).NotEmpty();
        RuleFor(x => x.Updates).NotEmpty();
        RuleForEach(x => x.Updates).ChildRules(u =>
        {
            u.RuleFor(x => x.Value).GreaterThanOrEqualTo(0);
        });
    }
}

public sealed class UpdateCountersCommandHandler(
    IAircraftRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateCountersCommand, Result>
{
    public async Task<Result> Handle(UpdateCountersCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure(Error.Forbidden("Organisation context is required."));

        var ac = await repository.GetByIdAsync(request.AircraftId, currentUser.OrganisationId.Value, ct);
        if (ac is null)
            return Result.Failure(Error.NotFound(nameof(Domain.Aggregates.Aircraft.Aircraft), request.AircraftId));

        foreach (var update in request.Updates)
        {
            var domainResult = ac.UpdateCounter(update.CounterType, update.Value, currentUser.UserId!.Value);
            if (domainResult.IsFailure)
                return Result.Failure(Error.Validation(domainResult.ErrorMessage!));
        }

        await repository.UpdateAsync(ac, ct);
        return Result.Success();
    }
}
