using Mro.Domain.Aggregates.Defect;
using Mro.Domain.Aggregates.Defect.Enums;

namespace Mro.Application.Abstractions;

public interface IDefectRepository
{
    Task<Domain.Aggregates.Defect.Defect?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);

    /// <summary>Checks whether a defect number is already in use within the organisation.</summary>
    Task<bool> ExistsAsync(string defectNumber, Guid organisationId, CancellationToken ct = default);

    Task<IReadOnlyList<Domain.Aggregates.Defect.Defect>> ListAsync(
        Guid organisationId,
        Guid? aircraftId,
        DefectStatus? status,
        DefectSeverity? severity,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<int> CountAsync(
        Guid organisationId,
        Guid? aircraftId,
        DefectStatus? status,
        DefectSeverity? severity,
        CancellationToken ct = default);

    Task AddAsync(Domain.Aggregates.Defect.Defect defect, CancellationToken ct = default);

    Task UpdateAsync(Domain.Aggregates.Defect.Defect defect, CancellationToken ct = default);
}
