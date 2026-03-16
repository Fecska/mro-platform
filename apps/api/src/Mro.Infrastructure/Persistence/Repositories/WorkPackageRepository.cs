using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Maintenance;
using Mro.Domain.Aggregates.Maintenance.Enums;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class WorkPackageRepository(AppDbContext db) : IWorkPackageRepository
{
    public async Task<WorkPackage?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<WorkPackage>()
            .Include("_items")
            .FirstOrDefaultAsync(wp => wp.Id == id && wp.OrganisationId == organisationId, ct);

    public async Task<int> CountAsync(Guid organisationId, CancellationToken ct = default) =>
        await db.Set<WorkPackage>().CountAsync(wp => wp.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<WorkPackage>> ListAsync(
        Guid organisationId,
        Guid? aircraftId,
        WorkPackageStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Set<WorkPackage>()
            .Include("_items")
            .Where(wp => wp.OrganisationId == organisationId);

        if (aircraftId.HasValue) query = query.Where(wp => wp.AircraftId == aircraftId.Value);
        if (status.HasValue)     query = query.Where(wp => wp.Status == status.Value);

        return await query
            .OrderByDescending(wp => wp.PlannedStartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(WorkPackage package, CancellationToken ct = default)
    {
        await db.Set<WorkPackage>().AddAsync(package, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(WorkPackage package, CancellationToken ct = default)
    {
        db.Set<WorkPackage>().Update(package);
        await db.SaveChangesAsync(ct);
    }
}
