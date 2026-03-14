using Mro.Domain.Events;

namespace Mro.Domain.Common.Audit;

/// <summary>
/// Base record for compliance-auditable domain events.
/// Extends the generic DomainEvent with actor context, organisation scoping,
/// and the affected entity reference — all required fields for Part-145 audit trails.
///
/// Application layer's IAuditService picks up events of this type and persists
/// them to the immutable audit_logs table.  The log is append-only; no UPDATE
/// or DELETE is ever issued against it.
///
/// Usage pattern:
///   All compliance-significant events (state transitions, sign-offs, hard stops)
///   should inherit from AuditDomainEvent rather than the plain DomainEvent.
/// </summary>
public abstract record AuditDomainEvent : DomainEvent
{
    /// <summary>
    /// UserId of the person whose action triggered this event.
    /// Must never be Guid.Empty — use a system-user sentinel value for automated actions.
    /// </summary>
    public required Guid ActorId { get; init; }

    /// <summary>Organisation that owns the affected entity.</summary>
    public required Guid OrganisationId { get; init; }

    /// <summary>
    /// The type of entity that changed (e.g. "WorkOrder", "Defect", "Release").
    /// Used for filtering and display in the audit log viewer.
    /// </summary>
    public required string EntityType { get; init; }

    /// <summary>
    /// Primary key of the affected entity.
    /// Null for entity-agnostic events such as authentication events.
    /// </summary>
    public Guid? EntityId { get; init; }

    /// <summary>
    /// One of the constants from <see cref="ComplianceEventType"/>.
    /// Stored as a string so historical records are not affected by future code changes.
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>IP address of the actor's request. Null for background/worker events.</summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// Human-readable description of the event, persisted verbatim in the audit log.
    /// Should be specific enough to be understood without additional context lookups.
    /// Example: "Work order WO-2024-0042 closed by John Smith (certifying staff)"
    /// </summary>
    public required string Description { get; init; }
}
