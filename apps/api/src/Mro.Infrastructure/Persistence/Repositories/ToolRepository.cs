using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Tool.Enums;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class ToolRepository(AppDbContext db) : IToolRepository
{
    public async Task<Mro.Domain.Aggregates.Tool.Tool?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Mro.Domain.Aggregates.Tool.Tool>()
            .Include("_calibrationRecords")
            .FirstOrDefaultAsync(t => t.Id == id && t.OrganisationId == organisationId, ct);

    public async Task<bool> ExistsAsync(string toolNumber, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<Mro.Domain.Aggregates.Tool.Tool>()
            .AnyAsync(t => t.ToolNumber == toolNumber.ToUpperInvariant() && t.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<Mro.Domain.Aggregates.Tool.Tool>> ListAsync(
        Guid organisationId, ToolStatus? status, ToolCategory? category, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Set<Mro.Domain.Aggregates.Tool.Tool>()
            .Where(t => t.OrganisationId == organisationId);

        if (status.HasValue)   query = query.Where(t => t.Status == status.Value);
        if (category.HasValue) query = query.Where(t => t.Category == category.Value);

        return await query
            .OrderBy(t => t.ToolNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Mro.Domain.Aggregates.Tool.Tool tool, CancellationToken ct = default)
    {
        await db.Set<Mro.Domain.Aggregates.Tool.Tool>().AddAsync(tool, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Mro.Domain.Aggregates.Tool.Tool tool, CancellationToken ct = default)
    {
        db.Set<Mro.Domain.Aggregates.Tool.Tool>().Update(tool);
        await db.SaveChangesAsync(ct);
    }
}
