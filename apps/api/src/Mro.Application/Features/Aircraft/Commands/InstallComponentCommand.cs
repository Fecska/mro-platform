using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Aircraft.Commands;

public sealed record InstallComponentCommand : IRequest<Result<Guid>>
{
    public required Guid AircraftId { get; init; }
    public required string PartNumber { get; init; }
    public required string SerialNumber { get; init; }
    public required string Description { get; init; }
    public required string InstallationPosition { get; init; }
    public Guid? WorkOrderId { get; init; }
    public Guid? InventoryItemId { get; init; }
}

public sealed class InstallComponentCommandValidator : AbstractValidator<InstallComponentCommand>
{
    public InstallComponentCommandValidator()
    {
        RuleFor(x => x.AircraftId).NotEmpty();
        RuleFor(x => x.PartNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.InstallationPosition).NotEmpty().MaximumLength(30);
    }
}

public sealed class InstallComponentCommandHandler(
    IAircraftRepository repository,
    ICurrentUserService currentUser)
    : IRequestHandler<InstallComponentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(InstallComponentCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var ac = await repository.GetByIdAsync(request.AircraftId, currentUser.OrganisationId.Value, ct);
        if (ac is null)
            return Result.Failure<Guid>(Error.NotFound(nameof(Domain.Aggregates.Aircraft.Aircraft), request.AircraftId));

        var actorId = currentUser.UserId!.Value;

        var domainResult = ac.InstallComponent(
            request.PartNumber,
            request.SerialNumber,
            request.Description,
            request.InstallationPosition,
            actorId,
            actorId,
            request.WorkOrderId,
            request.InventoryItemId);

        if (domainResult.IsFailure)
            return Result.Failure<Guid>(Error.Conflict(domainResult.ErrorMessage!));

        await repository.UpdateAsync(ac, ct);

        var installed = ac.InstalledComponents
            .First(c => c.SerialNumber == request.SerialNumber.Trim().ToUpperInvariant()
                     && c.PartNumber == request.PartNumber.Trim().ToUpperInvariant()
                     && c.IsInstalled);

        return Result.Success(installed.Id);
    }
}
