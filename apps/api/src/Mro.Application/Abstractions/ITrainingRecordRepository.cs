using Mro.Domain.Aggregates.Employee;

namespace Mro.Application.Abstractions;

public interface ITrainingRecordRepository
{
    Task<TrainingRecord?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);
    Task UpdateAsync(TrainingRecord record, CancellationToken ct = default);
}
