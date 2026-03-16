using Mro.Domain.Aggregates.Employee.Enums;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Employee;

/// <summary>
/// Records the outcome of a competency assessment for an employee.
///
/// Currency rules:
///   - An assessment is "current" when the result is Pass or Satisfactory
///     AND the NextReviewDate (if set) has not yet passed.
///   - Assessments without a NextReviewDate remain current indefinitely
///     (one-time qualifications such as initial type checks).
///   - An overdue review (NextReviewDate in the past) makes IsCurrent = false
///     and contributes to IsFullyCurrent = false on the Employee currency snapshot.
/// </summary>
public sealed class CompetencyAssessment : AuditableEntity
{
    public Guid EmployeeId { get; private set; }

    /// <summary>The user (instructor / assessor) who conducted the assessment.</summary>
    public Guid AssessorId { get; private set; }

    public DateOnly AssessmentDate { get; private set; }

    public AssessmentType AssessmentType { get; private set; }

    public AssessmentResult Result { get; private set; }

    /// <summary>Free-text observations, corrective actions, or pass conditions noted.</summary>
    public string? Comments { get; private set; }

    /// <summary>
    /// When a follow-up assessment is required (e.g. annual proficiency check).
    /// Null = one-time assessment; no scheduled re-check needed.
    /// </summary>
    public DateOnly? NextReviewDate { get; private set; }

    /// <summary>True when the review date has passed and no newer assessment supersedes this one.</summary>
    public bool IsReviewOverdue =>
        NextReviewDate.HasValue && NextReviewDate.Value < DateOnly.FromDateTime(DateTime.UtcNow.Date);

    /// <summary>
    /// True when the result is positive (Pass / Satisfactory) and the review is not overdue.
    /// Used by the currency engine to determine overall employee currency.
    /// </summary>
    public bool IsCurrent =>
        (Result == AssessmentResult.Pass || Result == AssessmentResult.Satisfactory)
        && !IsReviewOverdue;

    // EF Core
    private CompetencyAssessment() { }

    internal static CompetencyAssessment Create(
        Guid employeeId,
        Guid assessorId,
        DateOnly assessmentDate,
        AssessmentType assessmentType,
        AssessmentResult result,
        Guid organisationId,
        Guid actorId,
        string? comments = null,
        DateOnly? nextReviewDate = null)
    {
        return new CompetencyAssessment
        {
            EmployeeId     = employeeId,
            AssessorId     = assessorId,
            AssessmentDate = assessmentDate,
            AssessmentType = assessmentType,
            Result         = result,
            Comments       = comments?.Trim(),
            NextReviewDate = nextReviewDate,
            OrganisationId = organisationId,
            CreatedBy      = actorId,
            UpdatedBy      = actorId,
            CreatedAt      = DateTimeOffset.UtcNow,
            UpdatedAt      = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Corrects comments or adjusts the next review date after the record is saved
    /// (e.g. when the review date is rescheduled by QA).
    /// </summary>
    public DomainResult Update(DateOnly? nextReviewDate, bool clearNextReviewDate, string? comments, Guid actorId)
    {
        if (clearNextReviewDate)
            NextReviewDate = null;
        else if (nextReviewDate.HasValue)
            NextReviewDate = nextReviewDate;

        if (comments is not null)
            Comments = comments.Trim();

        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
        return DomainResult.Ok();
    }
}
