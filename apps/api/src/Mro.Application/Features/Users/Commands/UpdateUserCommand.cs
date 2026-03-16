using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Common.Permissions;

namespace Mro.Application.Features.Users.Commands;

// ── Command ──────────────────────────────────────────────────────────────────

/// <summary>
/// Patch-style update: only non-null fields are applied.
/// AssignRole / RemoveRole are handled separately via AddRoleCommand / RemoveRoleCommand
/// to keep audit trail granular. This command covers display name and active flag.
/// </summary>
public sealed record UpdateUserCommand : IRequest<Result>
{
    public required Guid UserId { get; init; }
    public string? DisplayName { get; init; }
    public bool? IsActive { get; init; }
    /// <summary>If provided, assigns this role (replaces existing assignment for same role name).</summary>
    public string? AssignRole { get; init; }
    public OperationalScope? AssignRoleScope { get; init; }
    /// <summary>If provided, revokes this role.</summary>
    public string? RemoveRole { get; init; }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.DisplayName).MaximumLength(200).When(x => x.DisplayName is not null);
        RuleFor(x => x.AssignRole)
            .Must(r => r is null || Roles.All.Contains(r))
            .WithMessage("Unknown role name.");
        RuleFor(x => x.RemoveRole)
            .Must(r => r is null || Roles.All.Contains(r))
            .WithMessage("Unknown role name.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class UpdateUserCommandHandler(
    IUserRepository users,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateUserCommand, Result>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure(Error.Forbidden("No organisation context."));

        var actorId = currentUser.UserId!.Value;
        var user    = await users.GetByIdAsync(request.UserId, ct);

        if (user is null || user.OrganisationId != currentUser.OrganisationId.Value)
            return Result.Failure(Error.NotFound(nameof(Domain.Aggregates.User.User), request.UserId));

        if (request.DisplayName is not null)
            user.UpdateDisplayName(request.DisplayName, actorId);

        if (request.IsActive == false)
            user.Deactivate(actorId, "Deactivated via admin update");
        else if (request.IsActive == true)
            user.Reactivate(actorId);

        if (request.AssignRole is not null)
            user.AssignRole(request.AssignRole, request.AssignRoleScope ?? OperationalScope.Unrestricted, actorId);

        if (request.RemoveRole is not null)
            user.RemoveRole(request.RemoveRole, actorId);

        await users.UpdateAsync(user, ct);
        return Result.Success();
    }
}
