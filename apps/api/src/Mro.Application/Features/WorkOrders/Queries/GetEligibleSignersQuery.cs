using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Employee.Enums;
using Mro.Domain.Aggregates.WorkOrder.Enums;

namespace Mro.Application.Features.WorkOrders.Queries;

// ── DTOs ─────────────────────────────────────────────────────────────────────

public sealed record EligibleAuthorisationDto(
    Guid   Id,
    string AuthorisationNumber,
    string Category,
    string Scope,
    string AircraftTypes,
    string? StationScope,
    DateOnly? ValidUntil);

public sealed record EligibleSignerDto(
    Guid   EmployeeId,
    string EmployeeNumber,
    string FullName,
    string Email,
    IReadOnlyList<EligibleAuthorisationDto> MatchingAuthorisations,

    /// <summary>True when the employee has at least one expired recurring training record.</summary>
    bool HasExpiredTraining,
    IReadOnlyList<string> TrainingWarnings);

public sealed record EligibleSignersDto(
    Guid   WorkOrderId,
    string WoNumber,
    string AircraftRegistration,
    string IcaoTypeCode,

    /// <summary>Required licence categories derived from active task RequiredLicence fields.</summary>
    IReadOnlyList<string> RequiredCategories,
    IReadOnlyList<EligibleSignerDto> Signers);

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetEligibleSignersQuery(Guid WorkOrderId)
    : IRequest<Result<EligibleSignersDto>>;

public sealed class GetEligibleSignersQueryHandler(
    IWorkOrderRepository workOrders,
    IAircraftRepository  aircraft,
    IEmployeeRepository  employees,
    ICurrentUserService  currentUser)
    : IRequestHandler<GetEligibleSignersQuery, Result<EligibleSignersDto>>
{
    public async Task<Result<EligibleSignersDto>> Handle(
        GetEligibleSignersQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<EligibleSignersDto>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        // ── 1. Load work order ────────────────────────────────────────────────

        var wo = await workOrders.GetByIdAsync(request.WorkOrderId, orgId, ct);
        if (wo is null)
            return Result.Failure<EligibleSignersDto>(Error.NotFound("WorkOrder", request.WorkOrderId));

        // ── 2. Resolve aircraft + ICAO type code ──────────────────────────────

        var ac = await aircraft.GetByIdAsync(wo.AircraftId, orgId, ct);
        if (ac is null)
            return Result.Failure<EligibleSignersDto>(
                Error.NotFound("Aircraft", wo.AircraftId));

        var acType = await aircraft.GetTypeByIdAsync(ac.AircraftTypeId, orgId, ct);
        var icaoCode = acType?.IcaoTypeCode ?? string.Empty;

        // ── 3. Required licence categories from active tasks ──────────────────

        var activeTasks = wo.Tasks
            .Where(t => t.Status != WorkOrderTaskStatus.Cancelled)
            .ToList();

        var requiredCategories = activeTasks
            .Where(t => !string.IsNullOrWhiteSpace(t.RequiredLicence))
            .Select(t => t.RequiredLicence!.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        // Parse to LicenceCategory enum for authorisation matching
        var parsedCategories = requiredCategories
            .Select(s => Enum.TryParse<LicenceCategory>(s, ignoreCase: true, out var cat) ? (LicenceCategory?)cat : null)
            .Where(c => c.HasValue)
            .Select(c => c!.Value)
            .ToHashSet();

        // ── 4. Load active employees with authorisations + training ───────────

        var activeEmployees = await employees.ListActiveWithAuthorisationsAsync(orgId, ct);

        // ── 5. Evaluate eligibility ────────────────────────────────────────────

        var signers = new List<EligibleSignerDto>();

        foreach (var emp in activeEmployees)
        {
            // a. Find current authorisations that cover the aircraft type and required category
            var matchingAuths = emp.Authorisations
                .Where(a => a.IsCurrent)
                .Where(a => CoversAircraftType(a.AircraftTypes, icaoCode))
                .Where(a => parsedCategories.Count == 0 || parsedCategories.Contains(a.Category))
                .ToList();

            if (matchingAuths.Count == 0)
                continue;

            // b. Training currency: check for expired recurring training
            var expiredTraining = emp.TrainingRecords
                .Where(t => t.IsRecurring && t.IsExpired)
                .ToList();

            var trainingWarnings = expiredTraining
                .Select(t => $"Training '{t.CourseCode}' expired on {t.ExpiresAt:yyyy-MM-dd}.")
                .ToList();

            signers.Add(new EligibleSignerDto(
                emp.Id,
                emp.EmployeeNumber,
                emp.FullName,
                emp.Email,
                matchingAuths.Select(a => new EligibleAuthorisationDto(
                    a.Id,
                    a.AuthorisationNumber,
                    a.Category.ToString(),
                    a.Scope,
                    a.AircraftTypes,
                    a.StationScope,
                    a.ValidUntil)).ToList(),
                HasExpiredTraining: expiredTraining.Count > 0,
                trainingWarnings));
        }

        return Result.Success(new EligibleSignersDto(
            wo.Id,
            wo.WoNumber,
            ac.Registration,
            icaoCode,
            requiredCategories,
            signers));
    }

    /// <summary>
    /// Returns true when the authorisation's AircraftTypes field covers the given ICAO code.
    /// An empty / whitespace-only AircraftTypes means the authorisation covers all types.
    /// </summary>
    private static bool CoversAircraftType(string aircraftTypes, string icaoCode)
    {
        if (string.IsNullOrWhiteSpace(aircraftTypes)) return true;
        if (string.IsNullOrWhiteSpace(icaoCode))      return true;

        return aircraftTypes
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(t => t.Equals(icaoCode, StringComparison.OrdinalIgnoreCase));
    }
}
