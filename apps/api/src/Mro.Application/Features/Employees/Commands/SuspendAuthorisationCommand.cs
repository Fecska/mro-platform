using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Commands;

public sealed class SuspendAuthorisationCommand : IRequest<Result<Unit>>
{
    public required Guid AuthorisationId { get; init; }
    public required string Reason { get; init; }
}

public sealed class SuspendAuthorisationCommandHandler(
    IAuthorisationRepository authorisations,
    ICurrentUserService currentUser)
    : IRequestHandler<SuspendAuthorisationCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SuspendAuthorisationCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var auth = await authorisations.GetByIdAsync(
            request.AuthorisationId, currentUser.OrganisationId.Value, ct);
        if (auth is null)
            return Result.Failure<Unit>(Error.NotFound("Authorisation", request.AuthorisationId));

        var domainResult = auth.Suspend(
            request.Reason, currentUser.UserId!.Value, currentUser.UserId!.Value);

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await authorisations.UpdateAsync(auth, ct);
        return Result.Success(Unit.Value);
    }
}

// ── Resume ────────────────────────────────────────────────────────────────────

public sealed class ResumeAuthorisationCommand : IRequest<Result<Unit>>
{
    public required Guid AuthorisationId { get; init; }
}

public sealed class ResumeAuthorisationCommandHandler(
    IAuthorisationRepository authorisations,
    ICurrentUserService currentUser)
    : IRequestHandler<ResumeAuthorisationCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(ResumeAuthorisationCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var auth = await authorisations.GetByIdAsync(
            request.AuthorisationId, currentUser.OrganisationId.Value, ct);
        if (auth is null)
            return Result.Failure<Unit>(Error.NotFound("Authorisation", request.AuthorisationId));

        var domainResult = auth.Resume(currentUser.UserId!.Value);

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await authorisations.UpdateAsync(auth, ct);
        return Result.Success(Unit.Value);
    }
}
