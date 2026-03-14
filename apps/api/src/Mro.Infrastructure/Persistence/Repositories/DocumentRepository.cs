using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Document;
using Mro.Domain.Aggregates.Document.Enums;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class DocumentRepository(AppDbContext db) : IDocumentRepository
{
    public async Task<MaintenanceDocument?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<MaintenanceDocument>()
            .Include("_revisions")
            .Include("_effectivities")
            .Include("_taskLinks")
            .FirstOrDefaultAsync(d => d.Id == id && d.OrganisationId == organisationId, ct);

    public async Task<bool> ExistsAsync(string documentNumber, DocumentType type, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<MaintenanceDocument>()
            .AnyAsync(d => d.DocumentNumber == documentNumber.ToUpperInvariant()
                        && d.DocumentType == type
                        && d.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<MaintenanceDocument>> ListAsync(
        Guid organisationId,
        DocumentType? type,
        DocumentStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Set<MaintenanceDocument>()
            .Include("_revisions")
            .Where(d => d.OrganisationId == organisationId);

        if (type.HasValue)   query = query.Where(d => d.DocumentType == type.Value);
        if (status.HasValue) query = query.Where(d => d.Status == status.Value);

        return await query
            .OrderBy(d => d.DocumentNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(Guid organisationId, DocumentType? type, DocumentStatus? status, CancellationToken ct = default)
    {
        var query = db.Set<MaintenanceDocument>().Where(d => d.OrganisationId == organisationId);
        if (type.HasValue)   query = query.Where(d => d.DocumentType == type.Value);
        if (status.HasValue) query = query.Where(d => d.Status == status.Value);
        return await query.CountAsync(ct);
    }

    public async Task AddAsync(MaintenanceDocument document, CancellationToken ct = default)
    {
        await db.Set<MaintenanceDocument>().AddAsync(document, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MaintenanceDocument document, CancellationToken ct = default)
    {
        db.Set<MaintenanceDocument>().Update(document);
        await db.SaveChangesAsync(ct);
    }
}
