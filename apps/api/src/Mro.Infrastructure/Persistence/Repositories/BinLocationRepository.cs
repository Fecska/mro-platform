using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Inventory;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class BinLocationRepository(AppDbContext db) : IBinLocationRepository
{
    public async Task<BinLocation?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<BinLocation>()
            .FirstOrDefaultAsync(b => b.Id == id && b.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<BinLocation>> ListAsync(Guid organisationId, CancellationToken ct = default) =>
        await db.Set<BinLocation>()
            .Where(b => b.OrganisationId == organisationId)
            .OrderBy(b => b.Code)
            .ToListAsync(ct);

    public async Task AddAsync(BinLocation location, CancellationToken ct = default)
    {
        await db.Set<BinLocation>().AddAsync(location, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BinLocation location, CancellationToken ct = default)
    {
        db.Set<BinLocation>().Update(location);
        await db.SaveChangesAsync(ct);
    }
}
