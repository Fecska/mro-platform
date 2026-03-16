using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Tool;

/// <summary>One calibration certificate entry for a tool.</summary>
public sealed class CalibrationRecord : AuditableEntity
{
    public Guid ToolId { get; private set; }
    public DateTimeOffset CalibratedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public string PerformedBy { get; private set; } = null!;
    public string? CertificateRef { get; private set; }
    public string? Notes { get; private set; }

    private CalibrationRecord() { }

    public static CalibrationRecord Create(
        Guid toolId,
        DateTimeOffset calibratedAt,
        DateTimeOffset expiresAt,
        string performedBy,
        Guid organisationId,
        Guid actorId,
        string? certificateRef = null,
        string? notes = null) => new()
    {
        ToolId         = toolId,
        CalibratedAt   = calibratedAt,
        ExpiresAt      = expiresAt,
        PerformedBy    = performedBy,
        CertificateRef = certificateRef,
        Notes          = notes,
        OrganisationId = organisationId,
        CreatedAt      = DateTimeOffset.UtcNow,
        CreatedBy      = actorId,
    };
}
