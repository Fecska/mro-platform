using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.User;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Set<User>()
            .Include("_roles")
            .Include("_refreshTokens")
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string email, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<User>()
            .Include("_roles")
            .Include("_refreshTokens")
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant()
                                   && u.OrganisationId == organisationId, ct);

    public async Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        await db.Set<User>()
            .Include("_roles")
            .Include("_refreshTokens")
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.TokenHash == tokenHash), ct);

    public async Task<IReadOnlyList<User>> ListAsync(
        Guid organisationId,
        string? role,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Set<User>()
            .Include("_roles")
            .Where(u => u.OrganisationId == organisationId);

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        if (!string.IsNullOrEmpty(role))
            query = query.Where(u => u.Roles.Any(r => r.RoleName == role && r.IsActive));

        return await query
            .OrderBy(u => u.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await db.Set<User>().AddAsync(user, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Set<User>().Update(user);
        await db.SaveChangesAsync(ct);
    }
}
