using Mro.Domain.Aggregates.Employee.Enums;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Employee;

/// <summary>
/// Records completion of a training course by an employee.
/// Required for regulatory compliance (Part-145 AMC 145.A.30, EWIS, CDCCL, etc.).
///
/// Recurring vs one-time:
///   - TrainingType.Recurrent / Refresher / Emergency / Simulator = periodic; ExpiresAt should be set.
///   - TrainingType.Initial = one-time qualification; ExpiresAt is typically null.
///   - IsRecurring is derived from TrainingType for convenience.
/// </summary>
public sealed class TrainingRecord : AuditableEntity
{
    public Guid EmployeeId { get; private set; }

    /// <summary>Internal or regulatory course code (e.g. "HF-001", "EWIS-PART66", "SMS-INITIAL").</summary>
    public string CourseCode { get; private set; } = string.Empty;

    public string CourseName { get; private set; } = string.Empty;

    public string TrainingProvider { get; private set; } = string.Empty;

    public TrainingType TrainingType { get; private set; }

    public DateOnly CompletedAt { get; private set; }

    /// <summary>
    /// Recurrency expiry. Null for one-time (Initial) courses.
    /// When set, IsExpired / IsRecurring reflect currency state.
    /// </summary>
    public DateOnly? ExpiresAt { get; private set; }

    /// <summary>
    /// Training outcome (e.g. "Pass", "Fail", "85%", "Satisfactory").
    /// Null if not formally assessed.
    /// </summary>
    public string? Result { get; private set; }

    /// <summary>Certificate or attendance record storage reference (path or ID).</summary>
    public string? CertificateRef { get; private set; }

    public bool IsExpired =>
        ExpiresAt.HasValue && ExpiresAt.Value < DateOnly.FromDateTime(DateTime.UtcNow.Date);

    /// <summary>
    /// True for Recurrent, Refresher, Emergency, and Simulator types — these require re-attendance.
    /// Initial training is a one-time qualification and IsRecurring is false.
    /// </summary>
    public bool IsRecurring => TrainingType != TrainingType.Initial;

    /// <summary>
    /// Updates mutable fields on an existing training record.
    /// ExpiresAt can be cleared (recurring → one-time reclassification) or extended.
    /// </summary>
    public DomainResult Update(
        DateOnly? expiresAt,
        bool clearExpiresAt,
        string? result,
        string? certificateRef,
        Guid actorId)
    {
        if (clearExpiresAt)
            ExpiresAt = null;
        else if (expiresAt.HasValue)
            ExpiresAt = expiresAt;

        if (result is not null)      Result         = result.Trim();
        if (certificateRef is not null) CertificateRef = certificateRef.Trim();

        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }

    // EF Core
    private TrainingRecord() { }

    internal static TrainingRecord Create(
        Guid employeeId,
        string courseCode,
        string courseName,
        string trainingProvider,
        TrainingType trainingType,
        DateOnly completedAt,
        Guid organisationId,
        Guid actorId,
        DateOnly? expiresAt = null,
        string? result = null,
        string? certificateRef = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(courseCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(courseName);
        ArgumentException.ThrowIfNullOrWhiteSpace(trainingProvider);

        return new TrainingRecord
        {
            EmployeeId       = employeeId,
            CourseCode       = courseCode.Trim().ToUpperInvariant(),
            CourseName       = courseName.Trim(),
            TrainingProvider = trainingProvider.Trim(),
            TrainingType     = trainingType,
            CompletedAt      = completedAt,
            ExpiresAt        = expiresAt,
            Result           = result?.Trim(),
            CertificateRef   = certificateRef?.Trim(),
            OrganisationId   = organisationId,
            CreatedBy        = actorId,
            UpdatedBy        = actorId,
            CreatedAt        = DateTimeOffset.UtcNow,
            UpdatedAt        = DateTimeOffset.UtcNow,
        };
    }
}
