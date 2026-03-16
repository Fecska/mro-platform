using Mro.Domain.Aggregates.Release;
using Mro.Domain.Aggregates.Release.Enums;

namespace Mro.Application.Abstractions;

public interface IReleaseCertificateRepository
{
    Task<ReleaseCertificate?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task<int> CountAsync(Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<ReleaseCertificate>> ListAsync(
        Guid organisationId,
        Guid? workOrderId,
        CertificateStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task AddAsync(ReleaseCertificate certificate, CancellationToken ct = default);
    Task UpdateAsync(ReleaseCertificate certificate, CancellationToken ct = default);
}
