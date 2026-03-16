using Mro.Domain.Aggregates.Inspection;
using Mro.Domain.Aggregates.Inspection.Enums;

namespace Mro.Application.Abstractions;

public interface IInspectionRepository
{
    Task<Inspection?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task<int> CountAsync(Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<Inspection>> ListAsync(
        Guid organisationId,
        Guid? workOrderId,
        InspectionStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task AddAsync(Inspection inspection, CancellationToken ct = default);
    Task UpdateAsync(Inspection inspection, CancellationToken ct = default);
}
