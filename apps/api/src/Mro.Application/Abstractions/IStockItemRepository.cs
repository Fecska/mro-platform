using Mro.Domain.Aggregates.Inventory;

namespace Mro.Application.Abstractions;

public interface IStockItemRepository
{
    Task<StockItem?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<StockItem>> ListAsync(Guid organisationId, Guid? partId, Guid? binLocationId, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(StockItem stockItem, CancellationToken ct = default);
    Task UpdateAsync(StockItem stockItem, CancellationToken ct = default);
}
