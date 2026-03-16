using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Defect.Enums;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class DefectRepository(AppDbContext db) : IDefectRepository
{
    public async Task<Domain.Aggregates.Defect.Defect?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Domain.Aggregates.Defect.Defect>()
            .Include("_actions")
            .Include("_deferrals")
            .FirstOrDefaultAsync(d => d.Id == id && d.OrganisationId == organisationId, ct);

    public async Task<bool> ExistsAsync(string defectNumber, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Domain.Aggregates.Defect.Defect>()
            .AnyAsync(d => d.DefectNumber == defectNumber.ToUpperInvariant()
                        && d.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<Domain.Aggregates.Defect.Defect>> ListAsync(
        Guid organisationId,
        Guid? aircraftId,
        DefectStatus? status,
        DefectSeverity? severity,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Set<Domain.Aggregates.Defect.Defect>()
            .Where(d => d.OrganisationId == organisationId);

        if (aircraftId.HasValue) query = query.Where(d => d.AircraftId == aircraftId.Value);
        if (status.HasValue)     query = query.Where(d => d.Status == status.Value);
        if (severity.HasValue)   query = query.Where(d => d.Severity == severity.Value);

        return await query
            .OrderByDescending(d => d.DiscoveredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(
        Guid organisationId,
        Guid? aircraftId,
        DefectStatus? status,
        DefectSeverity? severity,
        CancellationToken ct = default)
    {
        var query = db.Set<Domain.Aggregates.Defect.Defect>()
            .Where(d => d.OrganisationId == organisationId);

        if (aircraftId.HasValue) query = query.Where(d => d.AircraftId == aircraftId.Value);
        if (status.HasValue)     query = query.Where(d => d.Status == status.Value);
        if (severity.HasValue)   query = query.Where(d => d.Severity == severity.Value);

        return await query.CountAsync(ct);
    }

    public async Task AddAsync(Domain.Aggregates.Defect.Defect defect, CancellationToken ct = default)
    {
        await db.Set<Domain.Aggregates.Defect.Defect>().AddAsync(defect, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Domain.Aggregates.Defect.Defect defect, CancellationToken ct = default)
    {
        db.Set<Domain.Aggregates.Defect.Defect>().Update(defect);
        await db.SaveChangesAsync(ct);
    }
}
