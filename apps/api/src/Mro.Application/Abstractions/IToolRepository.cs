using Mro.Domain.Aggregates.Tool;
using Mro.Domain.Aggregates.Tool.Enums;

namespace Mro.Application.Abstractions;

public interface IToolRepository
{
    Task<Tool?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task<bool> ExistsAsync(string toolNumber, Guid organisationId, CancellationToken ct = default);
    Task<IReadOnlyList<Tool>> ListAsync(Guid organisationId, ToolStatus? status, ToolCategory? category, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Tool tool, CancellationToken ct = default);
    Task UpdateAsync(Tool tool, CancellationToken ct = default);
}
