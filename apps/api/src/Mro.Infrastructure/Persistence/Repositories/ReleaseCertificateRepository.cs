using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Release;
using Mro.Domain.Aggregates.Release.Enums;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class ReleaseCertificateRepository(AppDbContext db) : IReleaseCertificateRepository
{
    public async Task<ReleaseCertificate?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<ReleaseCertificate>()
            .Include("_signatures")
            .FirstOrDefaultAsync(c => c.Id == id && c.OrganisationId == organisationId, ct);

    public async Task<int> CountAsync(Guid organisationId, CancellationToken ct = default) =>
        await db.Set<ReleaseCertificate>().CountAsync(c => c.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<ReleaseCertificate>> ListAsync(
        Guid organisationId,
        Guid? workOrderId,
        CertificateStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Set<ReleaseCertificate>().Where(c => c.OrganisationId == organisationId);
        if (workOrderId.HasValue) query = query.Where(c => c.WorkOrderId == workOrderId.Value);
        if (status.HasValue)      query = query.Where(c => c.Status == status.Value);

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(ReleaseCertificate certificate, CancellationToken ct = default)
    {
        await db.Set<ReleaseCertificate>().AddAsync(certificate, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(ReleaseCertificate certificate, CancellationToken ct = default)
    {
        db.Set<ReleaseCertificate>().Update(certificate);
        await db.SaveChangesAsync(ct);
    }
}
