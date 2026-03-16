using Microsoft.EntityFrameworkCore;
using Mro.Application.Abstractions;
using Mro.Domain.Aggregates.Inventory;

namespace Mro.Infrastructure.Persistence.Repositories;

public sealed class StockItemRepository(AppDbContext db) : IStockItemRepository
{
    public async Task<StockItem?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default) =>
        await db.Set<StockItem>()
            .Include("_reservations")
            .Include("_issues")
            .Include("_returns")
            .FirstOrDefaultAsync(s => s.Id == id && s.OrganisationId == organisationId, ct);

    public async Task<IReadOnlyList<StockItem>> ListAsync(
        Guid organisationId, Guid? partId, Guid? binLocationId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Set<StockItem>()
            .Include("_reservations")
            .Where(s => s.OrganisationId == organisationId);

        if (partId.HasValue)        query = query.Where(s => s.PartId == partId.Value);
        if (binLocationId.HasValue) query = query.Where(s => s.BinLocationId == binLocationId.Value);

        return await query
            .OrderBy(s => s.PartId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task AddAsync(StockItem stockItem, CancellationToken ct = default)
    {
        await db.Set<StockItem>().AddAsync(stockItem, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(StockItem stockItem, CancellationToken ct = default)
    {
        db.Set<StockItem>().Update(stockItem);
        await db.SaveChangesAsync(ct);
    }
}
