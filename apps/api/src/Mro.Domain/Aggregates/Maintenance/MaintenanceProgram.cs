using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Maintenance;

/// <summary>
/// Aircraft Maintenance Programme (AMP) document registered for an aircraft type.
/// Acts as the master reference that DueItems are linked to.
/// </summary>
public sealed class MaintenanceProgram : AuditableEntity
{
    public string ProgramNumber { get; private set; } = null!;
    public string AircraftTypeCode { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string RevisionNumber { get; private set; } = null!;
    public DateOnly RevisionDate { get; private set; }
    public string? ApprovalReference { get; private set; }
    public bool IsActive { get; private set; } = true;

    private MaintenanceProgram() { }

    public static MaintenanceProgram Create(
        string programNumber,
        string aircraftTypeCode,
        string title,
        string revisionNumber,
        DateOnly revisionDate,
        Guid organisationId,
        Guid actorId,
        string? approvalReference = null) => new()
    {
        ProgramNumber     = programNumber.ToUpperInvariant(),
        AircraftTypeCode  = aircraftTypeCode.ToUpperInvariant(),
        Title             = title,
        RevisionNumber    = revisionNumber,
        RevisionDate      = revisionDate,
        ApprovalReference = approvalReference,
        OrganisationId    = organisationId,
        CreatedAt         = DateTimeOffset.UtcNow,
        CreatedBy         = actorId,
    };

    public void Revise(string newRevisionNumber, DateOnly newRevisionDate, Guid actorId)
    {
        RevisionNumber = newRevisionNumber;
        RevisionDate   = newRevisionDate;
        UpdatedAt      = DateTimeOffset.UtcNow;
        UpdatedBy      = actorId;
    }

    public void Deactivate(Guid actorId)
    {
        IsActive  = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
    }
}
