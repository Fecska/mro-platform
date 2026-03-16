using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Commands;

/// <summary>
/// Amends mutable scope fields of an existing authorisation.
/// Increments RevisionNumber so callers can detect concurrent changes.
/// </summary>
public sealed class AmendAuthorisationCommand : IRequest<Result<Unit>>
{
    public required Guid AuthorisationId { get; init; }
    public DateOnly? ValidUntil { get; init; }
    public bool ClearValidUntil { get; init; }
    public string? AircraftTypes { get; init; }
    public string? ComponentScope { get; init; }
    public string? StationScope { get; init; }
    public string? IssuingAuthority { get; init; }
}

public sealed class AmendAuthorisationCommandHandler(
    IAuthorisationRepository authorisations,
    ICurrentUserService currentUser)
    : IRequestHandler<AmendAuthorisationCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(AmendAuthorisationCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var auth = await authorisations.GetByIdAsync(
            request.AuthorisationId, currentUser.OrganisationId.Value, ct);
        if (auth is null)
            return Result.Failure<Unit>(Error.NotFound("Authorisation", request.AuthorisationId));

        var domainResult = auth.Amend(
            request.ValidUntil,
            request.ClearValidUntil,
            request.AircraftTypes,
            request.ComponentScope,
            request.StationScope,
            request.IssuingAuthority,
            currentUser.UserId!.Value);

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await authorisations.UpdateAsync(auth, ct);
        return Result.Success(Unit.Value);
    }
}
