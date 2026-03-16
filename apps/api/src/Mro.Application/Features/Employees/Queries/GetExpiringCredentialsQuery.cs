using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Employees.Queries;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed record ExpiringCredentialDto(
    Guid   EmployeeId,
    string EmployeeNumber,
    string FullName,

    /// <summary>"Licence" or "Authorisation"</summary>
    string CredentialType,

    /// <summary>Licence number or authorisation number.</summary>
    string Identifier,

    /// <summary>Licence category string (e.g. "B1", "C").</summary>
    string Category,

    DateOnly ExpiresOn,

    /// <summary>Negative when already expired.</summary>
    int DaysRemaining);

public sealed record ExpiringCredentialsDto(
    int Days,
    int TotalCount,
    int ExpiredCount,
    IReadOnlyList<ExpiringCredentialDto> Items);

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetExpiringCredentialsQuery(int Days = 30)
    : IRequest<Result<ExpiringCredentialsDto>>;

public sealed class GetExpiringCredentialsQueryHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<GetExpiringCredentialsQuery, Result<ExpiringCredentialsDto>>
{
    public async Task<Result<ExpiringCredentialsDto>> Handle(
        GetExpiringCredentialsQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<ExpiringCredentialsDto>(Error.Forbidden("Organisation context is required."));

        var days = request.Days is 30 or 60 or 90 ? request.Days : 30;
        var orgId = currentUser.OrganisationId.Value;
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var cutoff = today.AddDays(days);

        var activeEmployees = await employees.ListActiveWithLicencesAndAuthorisationsAsync(orgId, ct);

        var items = new List<ExpiringCredentialDto>();

        foreach (var emp in activeEmployees)
        {
            // Licences with an expiry date that falls on or before cutoff
            foreach (var lic in emp.Licences.Where(l => l.ExpiresAt.HasValue && l.ExpiresAt.Value <= cutoff))
            {
                var daysRemaining = lic.ExpiresAt!.Value.DayNumber - today.DayNumber;
                items.Add(new ExpiringCredentialDto(
                    emp.Id,
                    emp.EmployeeNumber,
                    emp.FullName,
                    "Licence",
                    lic.LicenceNumber,
                    lic.Category.ToString(),
                    lic.ExpiresAt.Value,
                    daysRemaining));
            }

            // Active (not revoked, not suspended) authorisations with a ValidUntil on or before cutoff
            foreach (var auth in emp.Authorisations.Where(a =>
                a.IsActive && !a.IsSuspended &&
                a.ValidUntil.HasValue && a.ValidUntil.Value <= cutoff))
            {
                var daysRemaining = auth.ValidUntil!.Value.DayNumber - today.DayNumber;
                items.Add(new ExpiringCredentialDto(
                    emp.Id,
                    emp.EmployeeNumber,
                    emp.FullName,
                    "Authorisation",
                    auth.AuthorisationNumber,
                    auth.Category.ToString(),
                    auth.ValidUntil.Value,
                    daysRemaining));
            }
        }

        // Sort: expired first (most negative), then by soonest expiry
        items.Sort((a, b) => a.DaysRemaining.CompareTo(b.DaysRemaining));

        var expiredCount = items.Count(i => i.DaysRemaining < 0);

        return Result.Success(new ExpiringCredentialsDto(
            days,
            items.Count,
            expiredCount,
            items));
    }
}
