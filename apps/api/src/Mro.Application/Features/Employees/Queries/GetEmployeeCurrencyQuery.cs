using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Queries;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed record LicenceCurrencyDto(
    Guid Id,
    string LicenceNumber,
    string Category,
    string? Subcategory,
    string IssuingAuthority,
    bool IsCurrent,
    DateOnly? ExpiresAt,
    int? DaysUntilExpiry);

public sealed record AuthorisationCurrencyDto(
    Guid Id,
    string AuthorisationNumber,
    string Category,
    string Scope,
    string AircraftTypes,
    bool IsCurrent,
    DateOnly? ValidUntil,
    int? DaysUntilExpiry);

public sealed record TrainingCurrencyDto(
    Guid Id,
    string CourseCode,
    string CourseName,
    DateOnly CompletedAt,
    bool IsCurrent,
    DateOnly? ExpiresAt,
    int? DaysUntilExpiry);

public sealed record AssessmentCurrencyDto(
    Guid Id,
    string AssessmentType,
    string Result,
    DateOnly AssessmentDate,
    bool IsCurrent,
    DateOnly? NextReviewDate,
    int? DaysUntilReview);

public sealed record EmployeeCurrencyDto(
    Guid EmployeeId,
    string EmployeeNumber,
    string FullName,
    string Status,
    bool HasCurrentLicence,
    bool HasCurrentAuthorisation,
    bool HasOverdueAssessment,
    bool IsFullyCurrent,
    IReadOnlyList<LicenceCurrencyDto> Licences,
    IReadOnlyList<AuthorisationCurrencyDto> Authorisations,
    IReadOnlyList<TrainingCurrencyDto> RecurrencyTraining,
    IReadOnlyList<AssessmentCurrencyDto> Assessments);

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetEmployeeCurrencyQuery(Guid EmployeeId)
    : IRequest<Result<EmployeeCurrencyDto>>;

public sealed class GetEmployeeCurrencyQueryHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<GetEmployeeCurrencyQuery, Result<EmployeeCurrencyDto>>
{
    public async Task<Result<EmployeeCurrencyDto>> Handle(GetEmployeeCurrencyQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<EmployeeCurrencyDto>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<EmployeeCurrencyDto>(Error.NotFound("Employee", request.EmployeeId));

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        static int? DaysUntil(DateOnly? d, DateOnly today) =>
            d.HasValue ? d.Value.DayNumber - today.DayNumber : null;

        var licenceDtos = emp.Licences.Select(l => new LicenceCurrencyDto(
            l.Id,
            l.LicenceNumber,
            l.Category.ToString(),
            l.Subcategory,
            l.IssuingAuthority,
            l.IsCurrent,
            l.ExpiresAt,
            DaysUntil(l.ExpiresAt, today))).ToList();

        var authDtos = emp.Authorisations
            .Where(a => a.IsActive)
            .Select(a => new AuthorisationCurrencyDto(
                a.Id,
                a.AuthorisationNumber,
                a.Category.ToString(),
                a.Scope,
                a.AircraftTypes,
                a.IsCurrent,
                a.ValidUntil,
                DaysUntil(a.ValidUntil, today))).ToList();

        // Only recurrency (expirable) training records are relevant for currency
        var trainingDtos = emp.TrainingRecords
            .Where(t => t.ExpiresAt.HasValue)
            .Select(t => new TrainingCurrencyDto(
                t.Id,
                t.CourseCode,
                t.CourseName,
                t.CompletedAt,
                !t.IsExpired,
                t.ExpiresAt,
                DaysUntil(t.ExpiresAt, today))).ToList();

        // Only assessments with a NextReviewDate participate in currency (periodic checks)
        var assessmentDtos = emp.CompetencyAssessments
            .Where(a => a.NextReviewDate.HasValue)
            .OrderByDescending(a => a.AssessmentDate)
            .Select(a => new AssessmentCurrencyDto(
                a.Id,
                a.AssessmentType.ToString(),
                a.Result.ToString(),
                a.AssessmentDate,
                a.IsCurrent,
                a.NextReviewDate,
                DaysUntil(a.NextReviewDate, today)))
            .ToList();

        var hasCurrentLicence       = licenceDtos.Any(l => l.IsCurrent);
        var hasCurrentAuthorisation = authDtos.Any(a => a.IsCurrent);
        var hasOverdueAssessment    = assessmentDtos.Any(a => !a.IsCurrent);

        var dto = new EmployeeCurrencyDto(
            emp.Id,
            emp.EmployeeNumber,
            $"{emp.FirstName} {emp.LastName}",
            emp.Status.ToString(),
            hasCurrentLicence,
            hasCurrentAuthorisation,
            hasOverdueAssessment,
            IsFullyCurrent: hasCurrentLicence && hasCurrentAuthorisation && !hasOverdueAssessment,
            licenceDtos,
            authDtos,
            trainingDtos,
            assessmentDtos);

        return Result.Success(dto);
    }
}
