using FluentValidation;
using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.User;
using Mro.Domain.Common.Permissions;

namespace Mro.Application.Features.Users.Commands;

// ── Command ──────────────────────────────────────────────────────────────────

public sealed record CreateUserCommand : IRequest<Result<Guid>>
{
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required string Password { get; init; }
    public string? InitialRole { get; init; }
    public OperationalScope? Scope { get; init; }
}

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.InitialRole)
            .Must(r => r is null || Roles.All.Contains(r))
            .WithMessage("Unknown role name.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CreateUserCommandHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("No organisation context."));

        var actorId  = currentUser.UserId!.Value;
        var orgId    = currentUser.OrganisationId.Value;

        // Duplicate email check within org
        var existing = await users.GetByEmailAsync(request.Email, orgId, ct);
        if (existing is not null)
            return Result.Failure<Guid>(Error.Conflict("A user with this email already exists in the organisation."));

        var hash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, request.DisplayName, hash, orgId, actorId);

        if (request.InitialRole is not null)
            user.AssignRole(request.InitialRole, request.Scope ?? OperationalScope.Unrestricted, actorId);

        await users.AddAsync(user, ct);
        return Result.Success(user.Id);
    }
}
