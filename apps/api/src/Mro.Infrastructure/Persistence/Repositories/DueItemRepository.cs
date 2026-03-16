using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Maintenance;
using Mro.Domain.Aggregates.Maintenance.Enums;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class DueItemRepository(AppDbContext db) : IDueItemRepository
{
    public async Task<DueItem?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<DueItem>()
            .FirstOrDefaultAsync(d => d.Id == id && d.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<DueItem>> ListAsync(
        Guid organisationId,
        Guid? aircraftId,
        DueStatus? status,
        DueItemType? type,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Set<DueItem>().Where(d => d.OrganisationId == organisationId);
        if (aircraftId.HasValue) query = query.Where(d => d.AircraftId == aircraftId.Value);
        if (status.HasValue)     query = query.Where(d => d.Status == status.Value);
        if (type.HasValue)       query = query.Where(d => d.DueItemType == type.Value);

        return await query
            .OrderBy(d => d.NextDueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(DueItem item, CancellationToken ct = default)
    {
        await db.Set<DueItem>().AddAsync(item, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(DueItem item, CancellationToken ct = default)
    {
        db.Set<DueItem>().Update(item);
        await db.SaveChangesAsync(ct);
    }
}
