namespace Mro.Domain.Entities;

/// <summary>
/// Extends BaseEntity with audit columns required for Part-145 record-keeping.
/// Every entity in the system that needs a compliance audit trail inherits from this.
///
/// Columns are populated by the EF Core AuditInterceptor — not set manually in business code.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>UTC timestamp of initial creation.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>User ID of the person who created the record.</summary>
    public Guid CreatedBy { get; set; }

    /// <summary>UTC timestamp of last modification.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>User ID of the last person to modify the record.</summary>
    public Guid UpdatedBy { get; set; }

    /// <summary>
    /// Organisation this record belongs to.
    /// Every entity is scoped to an organisation for future multi-tenancy isolation.
    /// </summary>
    public Guid OrganisationId { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null = active record.
    /// Hard delete is not permitted on compliance entities.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>User ID of the person who soft-deleted the record, if applicable.</summary>
    public Guid? DeletedBy { get; set; }

    public bool IsDeleted => DeletedAt.HasValue;
}
