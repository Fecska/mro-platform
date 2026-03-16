using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Commands;

public sealed class UpdateLicenceCommand : IRequest<Result<Unit>>
{
    public required Guid LicenceId { get; init; }
    public DateOnly? ExpiresAt { get; init; }
    public bool ClearExpiry { get; init; }
    public string? ScopeNotes { get; init; }
    public string? AttachmentRef { get; init; }
}

public sealed class UpdateLicenceCommandHandler(
    ILicenceRepository licences,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateLicenceCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateLicenceCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var licence = await licences.GetByIdAsync(
            request.LicenceId, currentUser.OrganisationId.Value, ct);
        if (licence is null)
            return Result.Failure<Unit>(Error.NotFound("Licence", request.LicenceId));

        // Expiry must be in the future when explicitly set
        if (request.ExpiresAt.HasValue
            && request.ExpiresAt.Value <= DateOnly.FromDateTime(DateTime.UtcNow.Date))
        {
            return Result.Failure<Unit>(
                Error.Validation("ExpiresAt must be a future date."));
        }

        var domainResult = licence.Update(
            request.ExpiresAt,
            request.ClearExpiry,
            request.ScopeNotes,
            request.AttachmentRef,
            currentUser.UserId!.Value);

        if (domainResult.IsFailure)
            return Result.Failure<Unit>(Error.Validation(domainResult.ErrorMessage!));

        await licences.UpdateAsync(licence, ct);
        return Result.Success(Unit.Value);
    }
}
