using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.WorkOrder;
using Mro.Domain.Aggregates.WorkOrder.Enums;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class WorkOrderRepository(AppDbContext db) : IWorkOrderRepository
{
    public async Task<WorkOrder?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<WorkOrder>()
            .Include("_tasks")
            .Include("_tasks._labourEntries")
            .Include("_tasks._requiredParts")
            .Include("_tasks._requiredTools")
            .Include("_assignments")
            .Include("_blockers")
            .FirstOrDefaultAsync(w => w.Id == id && w.OrganisationId == organisationId, ct);

    public async Task<bool> ExistsAsync(string woNumber, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<WorkOrder>()
            .AnyAsync(w => w.WoNumber == woNumber.ToUpperInvariant()
                        && w.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<WorkOrder>> ListAsync(
        Guid organisationId,
        Guid? aircraftId,
        WorkOrderStatus? status,
        WorkOrderType? type,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Set<WorkOrder>()
            .Include("_tasks")
            .Where(w => w.OrganisationId == organisationId);

        if (aircraftId.HasValue) query = query.Where(w => w.AircraftId == aircraftId.Value);
        if (status.HasValue)     query = query.Where(w => w.Status == status.Value);
        if (type.HasValue)       query = query.Where(w => w.WorkOrderType == type.Value);

        return await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        Guid organisationId,
        Guid? aircraftId,
        WorkOrderStatus? status,
        WorkOrderType? type,
        CancellationToken ct = default)
    {
        var query = db.Set<WorkOrder>().Where(w => w.OrganisationId == organisationId);
        if (aircraftId.HasValue) query = query.Where(w => w.AircraftId == aircraftId.Value);
        if (status.HasValue)     query = query.Where(w => w.Status == status.Value);
        if (type.HasValue)       query = query.Where(w => w.WorkOrderType == type.Value);
        return await query.CountAsync(ct);
    }

    public async Task AddAsync(WorkOrder workOrder, CancellationToken ct = default)
    {
        await db.Set<WorkOrder>().AddAsync(workOrder, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(WorkOrder workOrder, CancellationToken ct = default)
    {
        db.Set<WorkOrder>().Update(workOrder);
        await db.SaveChangesAsync(ct);
    }
}
