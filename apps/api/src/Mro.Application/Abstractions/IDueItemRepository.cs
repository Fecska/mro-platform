using Mro.Domain.Aggregates.Maintenance;
using Mro.Domain.Aggregates.Maintenance.Enums;

namespace Mro.Application.Abstractions;

public interface IDueItemRepository
{
    Task<DueItem?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<DueItem>> ListAsync(
        Guid organisationId,
        Guid? aircraftId,
        DueStatus? status,
        DueItemType? type,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task AddAsync(DueItem item, CancellationToken ct = default);
    Task UpdateAsync(DueItem item, CancellationToken ct = default);
}
