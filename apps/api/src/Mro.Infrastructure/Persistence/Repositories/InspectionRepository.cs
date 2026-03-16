using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Inspection;
using Mro.Domain.Aggregates.Inspection.Enums;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class InspectionRepository(AppDbContext db) : IInspectionRepository
{
    public async Task<Inspection?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Inspection>()
            .FirstOrDefaultAsync(i => i.Id == id && i.OrganisationId == organisationId, ct);

    public async Task<int> CountAsync(Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Inspection>().CountAsync(i => i.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<Inspection>> ListAsync(
        Guid organisationId,
        Guid? workOrderId,
        InspectionStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Set<Inspection>().Where(i => i.OrganisationId == organisationId);
        if (workOrderId.HasValue) query = query.Where(i => i.WorkOrderId == workOrderId.Value);
        if (status.HasValue)      query = query.Where(i => i.Status == status.Value);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Inspection inspection, CancellationToken ct = default)
    {
        await db.Set<Inspection>().AddAsync(inspection, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Inspection inspection, CancellationToken ct = default)
    {
        db.Set<Inspection>().Update(inspection);
        await db.SaveChangesAsync(ct);
    }
}
