using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Inventory;
using Mro.Domain.Aggregates.Inventory.Enums;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class PartRepository(AppDbContext db) : IPartRepository
{
    public async Task<Part?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Part>()
            .FirstOrDefaultAsync(p => p.Id == id && p.OrganisationId == organisationId, ct);

    public async Task<bool> ExistsAsync(string partNumber, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Part>()
            .AnyAsync(p => p.PartNumber == partNumber.ToUpperInvariant() && p.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<Part>> ListAsync(
        Guid organisationId, PartStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Set<Part>().Where(p => p.OrganisationId == organisationId);
        if (status.HasValue) query = query.Where(p => p.Status == status.Value);
        return await query
            .OrderBy(p => p.PartNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Part part, CancellationToken ct = default)
    {
        await db.Set<Part>().AddAsync(part, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Part part, CancellationToken ct = default)
    {
        db.Set<Part>().Update(part);
        await db.SaveChangesAsync(ct);
    }
}
