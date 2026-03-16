using Mro.Domain.Aggregates.Employee;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Abstractions;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, Guid organisationId, CancellationToken ct = default);

    /// <summary>
    /// Returns the employee linked to the given platform User account, with TrainingRecords loaded.
    /// Returns null when no employee record is associated with the user.
    /// </summary>
    Task<Employee?> GetByUserIdAsync(Guid userId, Guid organisationId, CancellationToken ct = default);

    Task<bool> ExistsAsync(string employeeNumber, Guid organisationId, CancellationToken ct = default);

    Task<(IReadOnlyList<Employee> Items, int Total)> ListAsync(
        Guid organisationId,
        EmployeeStatus? status,
        bool? isActive,
        LicenceCategory? licenceCategory,
        Guid? stationId,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<int> CountAsync(Guid organisationId, CancellationToken ct = default);

    /// <summary>
    /// Returns all Active employees with their Authorisations and TrainingRecords loaded.
    /// Used by the eligible-signers query — does NOT load shifts, assessments, or licences.
    /// </summary>
    Task<IReadOnlyList<Employee>> ListActiveWithAuthorisationsAsync(
        Guid organisationId, CancellationToken ct = default);

    /// <summary>
    /// Returns all Active employees with their Licences and Authorisations loaded.
    /// Used by the expiring-credentials dashboard query.
    /// </summary>
    Task<IReadOnlyList<Employee>> ListActiveWithLicencesAndAuthorisationsAsync(
        Guid organisationId, CancellationToken ct = default);

    /// <summary>
    /// Returns the employee with Attachments loaded (non-deleted only).
    /// Used by attach / remove-attachment commands.
    /// </summary>
    Task<Employee?> GetWithAttachmentsAsync(Guid id, Guid organisationId, CancellationToken ct = default);

    /// <summary>
    /// Returns all non-deleted attachments for the given employee, optionally filtered by type.
    /// </summary>
    Task<IReadOnlyList<EmployeeAttachment>> ListAttachmentsAsync(
        Guid employeeId,
        Guid organisationId,
        AttachmentType? type = null,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the employee with Restrictions loaded.
    /// Used by add/lift-restriction commands.
    /// </summary>
    Task<Employee?> GetWithRestrictionsAsync(Guid id, Guid organisationId, CancellationToken ct = default);

    /// <summary>
    /// Returns the employee with Notes loaded.
    /// Used by add/delete-note commands.
    /// </summary>
    Task<Employee?> GetWithNotesAsync(Guid id, Guid organisationId, CancellationToken ct = default);

    /// <summary>
    /// Returns all restrictions for the given employee, optionally filtered to active-only.
    /// </summary>
    Task<IReadOnlyList<EmployeeRestriction>> ListRestrictionsAsync(
        Guid employeeId,
        Guid organisationId,
        bool activeOnly = false,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all non-deleted notes for the given employee.
    /// </summary>
    Task<IReadOnlyList<EmployeeNote>> ListNotesAsync(
        Guid employeeId,
        Guid organisationId,
        CancellationToken ct = default);

    /// <summary>
    /// Fetches all employee-related sub-entity records needed to build the activity timeline.
    /// Shifts and training records are filtered to those on or after <paramref name="from"/>.
    /// Licences, authorisations, and restrictions are returned in full (typically small sets).
    /// </summary>
    Task<(
        IReadOnlyList<Licence>              Licences,
        IReadOnlyList<Authorisation>        Authorisations,
        IReadOnlyList<TrainingRecord>       TrainingRecords,
        IReadOnlyList<Shift>                Shifts,
        IReadOnlyList<EmployeeRestriction>  Restrictions
    )> GetForTimelineAsync(
        Guid employeeId,
        Guid organisationId,
        DateOnly from,
        CancellationToken ct = default);

    Task AddAsync(Employee employee, CancellationToken ct = default);

    Task UpdateAsync(Employee employee, CancellationToken ct = default);
}
