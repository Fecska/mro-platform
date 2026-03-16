using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;

namespace Mro.Application.Features.Employees.Queries;

/// <summary>
/// Returns a chronological activity timeline for a single employee.
///
/// Sources aggregated (most-recent-first):
///   • Licence added / updated (revalidation, type rating)
///   • Training recorded (completion date)
///   • Authorisation granted / suspended / revoked
///   • Shift recorded (shift date)
///   • Restriction added / lifted
///
/// Events older than <see cref="DaysBack"/> days are excluded.
/// The result is capped at <see cref="Limit"/> entries.
/// </summary>
public sealed class GetEmployeeTimelineQuery : IRequest<Result<IReadOnlyList<TimelineEventDto>>>
{
    public required Guid EmployeeId { get; init; }

    /// <summary>How many calendar days back to include (default: 90).</summary>
    public int DaysBack { get; init; } = 90;

    /// <summary>Maximum number of events to return (default: 50, max: 200).</summary>
    public int Limit { get; init; } = 50;
}

public sealed record TimelineEventDto(
    DateTimeOffset EventAt,
    TimelineEventType EventType,
    string Title,
    string? Detail,
    Guid? EntityId,
    Guid ActorId);

public sealed class GetEmployeeTimelineQueryHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<GetEmployeeTimelineQuery, Result<IReadOnlyList<TimelineEventDto>>>
{
    public async Task<Result<IReadOnlyList<TimelineEventDto>>> Handle(
        GetEmployeeTimelineQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<IReadOnlyList<TimelineEventDto>>(
                Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        // Verify employee exists
        var exists = await employees.GetByIdAsync(request.EmployeeId, orgId, ct);
        if (exists is null)
            return Result.Failure<IReadOnlyList<TimelineEventDto>>(
                Error.NotFound("Employee", request.EmployeeId));

        var daysBack  = Math.Clamp(request.DaysBack, 1, 365);
        var limit     = Math.Clamp(request.Limit, 1, 200);
        var from      = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-daysBack));
        var fromOffset = Utc(from);

        var (licences, authorisations, trainingRecords, shifts, restrictions) =
            await employees.GetForTimelineAsync(request.EmployeeId, orgId, from, ct);

        var events = new List<TimelineEventDto>();

        // ── Licences ─────────────────────────────────────────────────────────
        foreach (var l in licences)
        {
            if (l.CreatedAt >= fromOffset)
            {
                events.Add(new TimelineEventDto(
                    l.CreatedAt,
                    TimelineEventType.LicenceAdded,
                    $"Licence {l.LicenceNumber} added",
                    $"{l.Category}{(l.Subcategory is not null ? " " + l.Subcategory : "")} – {l.IssuingAuthority}",
                    l.Id,
                    l.CreatedBy));
            }

            // Detect revalidation / update (UpdatedAt more than 1 min after creation)
            if (l.UpdatedAt > l.CreatedAt.AddMinutes(1) && l.UpdatedAt >= fromOffset)
            {
                events.Add(new TimelineEventDto(
                    l.UpdatedAt,
                    TimelineEventType.LicenceUpdated,
                    $"Licence {l.LicenceNumber} updated",
                    l.ExpiresAt.HasValue
                        ? $"Expires: {l.ExpiresAt.Value:yyyy-MM-dd}"
                        : null,
                    l.Id,
                    l.UpdatedBy));
            }
        }

        // ── Authorisations ────────────────────────────────────────────────────
        foreach (var a in authorisations)
        {
            if (a.CreatedAt >= fromOffset)
            {
                events.Add(new TimelineEventDto(
                    a.CreatedAt,
                    TimelineEventType.AuthorisationGranted,
                    $"Authorisation {a.AuthorisationNumber} granted",
                    $"{a.Scope} / {a.Category}",
                    a.Id,
                    a.CreatedBy));
            }

            if (a.SuspendedAt.HasValue && a.SuspendedAt.Value >= fromOffset)
            {
                events.Add(new TimelineEventDto(
                    a.SuspendedAt.Value,
                    TimelineEventType.AuthorisationSuspended,
                    $"Authorisation {a.AuthorisationNumber} suspended",
                    a.SuspensionReason,
                    a.Id,
                    a.SuspendedByUserId ?? a.UpdatedBy));
            }

            if (a.RevokedAt.HasValue && a.RevokedAt.Value >= fromOffset)
            {
                events.Add(new TimelineEventDto(
                    a.RevokedAt.Value,
                    TimelineEventType.AuthorisationRevoked,
                    $"Authorisation {a.AuthorisationNumber} revoked",
                    a.RevocationReason,
                    a.Id,
                    a.RevokedByUserId ?? a.UpdatedBy));
            }
        }

        // ── Training records ──────────────────────────────────────────────────
        foreach (var t in trainingRecords)
        {
            events.Add(new TimelineEventDto(
                Utc(t.CompletedAt),
                TimelineEventType.TrainingRecorded,
                $"{t.CourseName} training completed",
                $"{t.TrainingType} – {t.TrainingProvider}" +
                    (t.ExpiresAt.HasValue ? $" (expires {t.ExpiresAt.Value:yyyy-MM-dd})" : string.Empty),
                t.Id,
                t.CreatedBy));
        }

        // ── Shifts ────────────────────────────────────────────────────────────
        foreach (var s in shifts)
        {
            events.Add(new TimelineEventDto(
                Utc(s.ShiftDate),
                TimelineEventType.ShiftRecorded,
                $"{s.ShiftType} shift",
                $"{s.StartTime:HH:mm}–{s.EndTime:HH:mm}",
                s.Id,
                s.CreatedBy));
        }

        // ── Restrictions ──────────────────────────────────────────────────────
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var r in restrictions)
        {
            if (r.CreatedAt >= fromOffset)
            {
                events.Add(new TimelineEventDto(
                    r.CreatedAt,
                    TimelineEventType.RestrictionAdded,
                    $"{FormatRestrictionType(r.RestrictionType)} restriction added",
                    r.Details,
                    r.Id,
                    r.CreatedBy));
            }

            // Detect lifted: ActiveUntil was set to a past date after creation
            if (r.ActiveUntil.HasValue
                && r.ActiveUntil.Value < today
                && r.UpdatedAt > r.CreatedAt.AddMinutes(1)
                && r.UpdatedAt >= fromOffset)
            {
                events.Add(new TimelineEventDto(
                    r.UpdatedAt,
                    TimelineEventType.RestrictionLifted,
                    $"{FormatRestrictionType(r.RestrictionType)} restriction lifted",
                    null,
                    r.Id,
                    r.UpdatedBy));
            }
        }

        var result = events
            .OrderByDescending(e => e.EventAt)
            .Take(limit)
            .ToList();

        return Result.Success<IReadOnlyList<TimelineEventDto>>(result);
    }

    private static DateTimeOffset Utc(DateOnly d) =>
        new(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    private static string FormatRestrictionType(RestrictionType type) => type switch
    {
        RestrictionType.TemporarySuspension => "Temporary suspension",
        RestrictionType.SupervisedWorkOnly  => "Supervised work only",
        RestrictionType.StationRestricted   => "Station",
        RestrictionType.NoReleasePrivilege  => "No release privilege",
        _                                   => "Other",
    };
}
