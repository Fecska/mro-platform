using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Queries;

// ── Status enum ───────────────────────────────────────────────────────────────

/// <summary>
/// Aggregated currency traffic-light for a single employee.
/// Green = fully current; Amber = action needed soon; Red = immediate action required.
/// </summary>
public enum CurrencyStatus { Green, Amber, Red }

// ── DTO ───────────────────────────────────────────────────────────────────────

public sealed record CurrencyStatusDto(
    Guid   EmployeeId,
    string EmployeeNumber,
    string FullName,
    string EmployeeStatus,
    CurrencyStatus Status,

    /// <summary>Conditions that caused a Red rating. Non-empty ⇒ Status == Red.</summary>
    IReadOnlyList<string> RedReasons,

    /// <summary>Conditions that caused an Amber rating. Non-empty when Status == Amber.</summary>
    IReadOnlyList<string> AmberReasons,

    /// <summary>True when the employee is within a rostered shift right now (UTC).</summary>
    bool IsOnShiftNow);

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetEmployeeCurrencyStatusQuery(Guid EmployeeId)
    : IRequest<Result<CurrencyStatusDto>>;

public sealed class GetEmployeeCurrencyStatusQueryHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<GetEmployeeCurrencyStatusQuery, Result<CurrencyStatusDto>>
{
    // Threshold: warn N days before expiry (Amber boundary)
    private const int ExpiryWarnDays = 30;

    public async Task<Result<CurrencyStatusDto>> Handle(
        GetEmployeeCurrencyStatusQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<CurrencyStatusDto>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.EmployeeId, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<CurrencyStatusDto>(Error.NotFound("Employee", request.EmployeeId));

        var today   = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var nowTime = TimeOnly.FromDateTime(DateTime.UtcNow);

        var red   = new List<string>();
        var amber = new List<string>();

        // ── 1. Active status ──────────────────────────────────────────────────

        if (emp.Status == EmployeeStatus.Terminated)
            red.Add("Employee is terminated.");
        else if (emp.Status == EmployeeStatus.Suspended)
            amber.Add("Employee is suspended.");

        // ── 2. Licence currency ───────────────────────────────────────────────

        var currentLicences = emp.Licences.Where(l => l.IsCurrent).ToList();
        if (currentLicences.Count == 0)
        {
            red.Add("No valid licence on file.");
        }
        else
        {
            foreach (var l in currentLicences.Where(l => l.ExpiresAt.HasValue))
            {
                var days = l.ExpiresAt!.Value.DayNumber - today.DayNumber;
                if (days <= ExpiryWarnDays)
                    amber.Add($"Licence '{l.LicenceNumber}' expires in {days} day(s) ({l.ExpiresAt:yyyy-MM-dd}).");
            }
        }

        // ── 3. Authorisation currency ─────────────────────────────────────────

        var currentAuthorisations = emp.Authorisations.Where(a => a.IsCurrent).ToList();
        if (currentAuthorisations.Count == 0)
        {
            red.Add("No valid authorisation on file.");
        }
        else
        {
            foreach (var a in currentAuthorisations.Where(a => a.ValidUntil.HasValue))
            {
                var days = a.ValidUntil!.Value.DayNumber - today.DayNumber;
                if (days <= ExpiryWarnDays)
                    amber.Add($"Authorisation '{a.AuthorisationNumber}' expires in {days} day(s) ({a.ValidUntil:yyyy-MM-dd}).");
            }
        }

        // ── 4. Recurring training currency ────────────────────────────────────

        var recurringTraining = emp.TrainingRecords.Where(t => t.IsRecurring && t.ExpiresAt.HasValue).ToList();
        foreach (var t in recurringTraining)
        {
            var days = t.ExpiresAt!.Value.DayNumber - today.DayNumber;
            if (days < 0)
                red.Add($"Training '{t.CourseCode}' expired on {t.ExpiresAt:yyyy-MM-dd}.");
            else if (days <= ExpiryWarnDays)
                amber.Add($"Training '{t.CourseCode}' expires in {days} day(s) ({t.ExpiresAt:yyyy-MM-dd}).");
        }

        // ── 5. Competency assessment currency ─────────────────────────────────

        var periodicAssessments = emp.CompetencyAssessments.Where(a => a.NextReviewDate.HasValue).ToList();
        foreach (var a in periodicAssessments)
        {
            var days = a.NextReviewDate!.Value.DayNumber - today.DayNumber;
            if (days < 0)
                red.Add($"Competency assessment review overdue: {a.AssessmentType} (due {a.NextReviewDate:yyyy-MM-dd}).");
            else if (days <= ExpiryWarnDays)
                amber.Add($"Competency assessment review due in {days} day(s): {a.AssessmentType} ({a.NextReviewDate:yyyy-MM-dd}).");
        }

        // ── 6. Shift presence ─────────────────────────────────────────────────

        var isOnShiftNow = emp.Shifts.Any(s =>
            s.ShiftDate == today &&
            s.StartTime <= nowTime &&
            s.EndTime   >  nowTime &&
            s.AvailabilityStatus == AvailabilityStatus.Available);

        if (!isOnShiftNow)
            amber.Add("Employee has no active shift at this time.");

        // ── Aggregate status ──────────────────────────────────────────────────

        var status = red.Count > 0  ? CurrencyStatus.Red
                   : amber.Count > 0 ? CurrencyStatus.Amber
                   : CurrencyStatus.Green;

        return Result.Success(new CurrencyStatusDto(
            emp.Id,
            emp.EmployeeNumber,
            emp.FullName,
            emp.Status.ToString(),
            status,
            red.AsReadOnly(),
            amber.AsReadOnly(),
            isOnShiftNow));
    }
}
