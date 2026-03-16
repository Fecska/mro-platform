using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Employee;

/// <summary>
/// A free-text note attached to an employee record (e.g. "Pending medical renewal", "Commended for safety report").
///
/// Notes are immutable after creation — to correct a note, delete it and add a new one.
/// Confidential notes should only be visible to managers/HR roles (enforcement at API/query layer).
/// </summary>
public sealed class EmployeeNote : AuditableEntity
{
    public Guid EmployeeId { get; private set; }

    /// <summary>Note body — max 2000 characters.</summary>
    public string NoteText { get; private set; } = string.Empty;

    /// <summary>
    /// When true, the note should only be visible to authorised roles (managers, HR).
    /// Regular supervisors and the employee themselves cannot see confidential notes.
    /// </summary>
    public bool IsConfidential { get; private set; }

    // EF Core
    private EmployeeNote() { }

    internal static EmployeeNote Create(
        Guid employeeId,
        string noteText,
        bool isConfidential,
        Guid organisationId,
        Guid actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(noteText);

        return new EmployeeNote
        {
            EmployeeId     = employeeId,
            NoteText       = noteText.Trim(),
            IsConfidential = isConfidential,
            OrganisationId = organisationId,
            CreatedBy      = actorId,
            UpdatedBy      = actorId,
            CreatedAt      = DateTimeOffset.UtcNow,
            UpdatedAt      = DateTimeOffset.UtcNow,
        };
    }

    internal void SoftDelete(Guid actorId)
    {
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
    }
}
