using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using AircraftAggregate = Mro.Domain.Aggregates.Aircraft.Aircraft;
using Mro.Domain.Aggregates.Aircraft;

namespace Mro.Application.Features.Aircraft.Commands;

public sealed record RegisterAircraftCommand : IRequest<Result<Guid>>
{
    public required string Registration { get; init; }
    public required string SerialNumber { get; init; }
    public required Guid AircraftTypeId { get; init; }
    public required DateOnly ManufactureDate { get; init; }
    public string? Remarks { get; init; }
}

public sealed class RegisterAircraftCommandValidator : AbstractValidator<RegisterAircraftCommand>
{
    public RegisterAircraftCommandValidator()
    {
        RuleFor(x => x.Registration).NotEmpty().MaximumLength(10);
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.AircraftTypeId).NotEmpty();
        RuleFor(x => x.ManufactureDate).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}

public sealed class RegisterAircraftCommandHandler(
    IAircraftRepository aircraft,
    ICurrentUserService currentUser)
    : IRequestHandler<RegisterAircraftCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterAircraftCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        var type = await aircraft.GetTypeByIdAsync(request.AircraftTypeId, orgId, ct);
        if (type is null)
            return Result.Failure<Guid>(Error.NotFound(nameof(Domain.Aggregates.Aircraft.AircraftType), request.AircraftTypeId));

        var existing = await aircraft.GetByRegistrationAsync(request.Registration, orgId, ct);
        if (existing is not null)
            return Result.Failure<Guid>(Error.Conflict($"Registration '{request.Registration}' already exists."));

        var ac = AircraftAggregate.Register(
            request.Registration,
            request.SerialNumber,
            request.AircraftTypeId,
            request.ManufactureDate,
            orgId,
            currentUser.UserId!.Value,
            request.Remarks);

        await aircraft.AddAsync(ac, ct);
        return Result.Success(ac.Id);
    }
}
