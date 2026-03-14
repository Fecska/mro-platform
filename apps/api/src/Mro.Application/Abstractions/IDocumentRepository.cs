using Mro.Domain.Aggregates.Document;
using Mro.Domain.Aggregates.Document.Enums;

namespace Mro.Application.Abstractions;

public interface IDocumentRepository
{
    Task<MaintenanceDocument?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);

    Task<bool> ExistsAsync(string documentNumber, DocumentType type, Guid organisationId, CancellationToken ct = default);

    Task<IReadOnlyList<MaintenanceDocument>> ListAsync(
        Guid organisationId,
        DocumentType? type = null,
        DocumentStatus? status = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default);

    Task<int> CountAsync(Guid organisationId, DocumentType? type, DocumentStatus? status, CancellationToken ct = default);

    Task AddAsync(MaintenanceDocument document, CancellationToken ct = default);
    Task UpdateAsync(MaintenanceDocument document, CancellationToken ct = default);
}
