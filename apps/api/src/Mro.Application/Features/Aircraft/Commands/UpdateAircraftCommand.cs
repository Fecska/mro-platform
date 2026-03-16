using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Aircraft.Commands;

public sealed record UpdateAircraftCommand : IRequest<Result>
{
    public required Guid AircraftId { get; init; }
    /// <summary>Null means "do not change"; empty string clears the remarks field.</summary>
    public string? Remarks { get; init; }
    public bool UpdateRemarks { get; init; }   // explicit flag to distinguish null-as-clear vs not-provided
}

public sealed class UpdateAircraftCommandValidator : AbstractValidator<UpdateAircraftCommand>
{
    public UpdateAircraftCommandValidator()
    {
        RuleFor(x => x.AircraftId).NotEmpty();
        RuleFor(x => x.Remarks).MaximumLength(500).When(x => x.Remarks is not null);
    }
}

public sealed class UpdateAircraftCommandHandler(
    IAircraftRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateAircraftCommand, Result>
{
    public async Task<Result> Handle(UpdateAircraftCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure(Error.Forbidden("Organisation context is required."));

        var ac = await repository.GetByIdAsync(request.AircraftId, currentUser.OrganisationId.Value, ct);
        if (ac is null)
            return Result.Failure(Error.NotFound(nameof(Domain.Aggregates.Aircraft.Aircraft), request.AircraftId));

        if (request.UpdateRemarks)
            ac.UpdateRemarks(request.Remarks, currentUser.UserId!.Value);

        await repository.UpdateAsync(ac, ct);
        return Result.Success();
    }
}
