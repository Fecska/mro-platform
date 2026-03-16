using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Users.Commands;
using Mro.Application.Features.Users.Queries;
using Mro.Domain.Common.Permissions;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(ISender sender) : ControllerBase
{
    // ── GET /api/users ────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListUsersQuery(role, isActive, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/users ───────────────────────────────────────────────────

    public sealed record CreateUserRequest(
        string Email,
        string DisplayName,
        string Password,
        string? InitialRole,
        IReadOnlyList<Guid>? StationIds,
        IReadOnlyList<string>? AircraftTypes,
        IReadOnlyList<string>? ReleaseCategories);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        OperationalScope? scope = null;
        if (request.InitialRole is not null)
        {
            scope = new OperationalScope
            {
                StationIds       = request.StationIds       ?? [],
                AircraftTypes    = request.AircraftTypes    ?? [],
                ReleaseCategories = request.ReleaseCategories ?? [],
            };
        }

        var result = await sender.Send(new CreateUserCommand
        {
            Email       = request.Email,
            DisplayName = request.DisplayName,
            Password    = request.Password,
            InitialRole = request.InitialRole,
            Scope       = scope,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/users/{id} ───────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetUserQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── PATCH /api/users/{id} ─────────────────────────────────────────────

    public sealed record PatchUserRequest(
        string? DisplayName,
        bool? IsActive,
        string? AssignRole,
        IReadOnlyList<Guid>? AssignRoleStationIds,
        IReadOnlyList<string>? AssignRoleAircraftTypes,
        IReadOnlyList<string>? AssignRoleReleaseCategories,
        string? RemoveRole);

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PatchUserRequest request, CancellationToken ct)
    {
        OperationalScope? scope = null;
        if (request.AssignRole is not null)
        {
            scope = new OperationalScope
            {
                StationIds        = request.AssignRoleStationIds        ?? [],
                AircraftTypes     = request.AssignRoleAircraftTypes     ?? [],
                ReleaseCategories = request.AssignRoleReleaseCategories ?? [],
            };
        }

        var result = await sender.Send(new UpdateUserCommand
        {
            UserId          = id,
            DisplayName     = request.DisplayName,
            IsActive        = request.IsActive,
            AssignRole      = request.AssignRole,
            AssignRoleScope = scope,
            RemoveRole      = request.RemoveRole,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }
}

// ── Separate controller for roles / permissions (read-only reference data) ──

[ApiController]
[Authorize]
public sealed class RolesController(ISender sender) : ControllerBase
{
    [HttpGet("api/roles")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await sender.Send(new ListRolesQuery(), ct);
        return Ok(result.Value);
    }
}

[ApiController]
[Authorize]
public sealed class PermissionsController(ISender sender) : ControllerBase
{
    [HttpGet("api/permissions")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await sender.Send(new ListPermissionsQuery(), ct);
        return Ok(result.Value);
    }
}
