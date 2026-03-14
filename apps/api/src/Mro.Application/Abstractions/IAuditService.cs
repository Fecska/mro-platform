namespace Mro.Application.Abstractions;

/// <summary>
/// Writes immutable audit events.
/// All compliance-critical actions must produce an audit event.
/// The audit log is append-only — no updates or deletes permitted.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Records a compliance action (release, inspection sign-off, authorisation grant, etc.)
    /// These events are given higher retention priority and cannot be pruned.
    /// </summary>
    Task RecordComplianceEventAsync(
        string action,
        string entityType,
        Guid entityId,
        object? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a standard data modification event (create, update, soft-delete).
    /// Called automatically by AuditInterceptor — not needed in business code.
    /// </summary>
    Task RecordDataChangeAsync(
        string action,
        string entityType,
        Guid entityId,
        object? oldValues,
        object? newValues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a security event (login, logout, permission denied, MFA failure, etc.)
    /// </summary>
    Task RecordSecurityEventAsync(
        string action,
        string? targetUserId = null,
        object? context = null,
        CancellationToken cancellationToken = default);
}
