using Mro.Domain.Aggregates.WorkOrder;
using Mro.Domain.Aggregates.WorkOrder.Enums;

namespace Mro.Application.Abstractions;

public interface IWorkOrderRepository
{
    Task<WorkOrder?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);

    Task<bool> ExistsAsync(string woNumber, Guid organisationId, CancellationToken ct = default);

    Task<IReadOnlyList<WorkOrder>> ListAsync(
        Guid organisationId,
        Guid? aircraftId,
        WorkOrderStatus? status,
        WorkOrderType? type,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<int> CountAsync(
        Guid organisationId,
        Guid? aircraftId,
        WorkOrderStatus? status,
        WorkOrderType? type,
        CancellationToken ct = default);

    Task AddAsync(WorkOrder workOrder, CancellationToken ct = default);

    Task UpdateAsync(WorkOrder workOrder, CancellationToken ct = default);
}
