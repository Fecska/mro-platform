using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Application.Features.Employees.Dtos;

namespace Mro.Application.Features.Employees.Queries;

public sealed record GetEmployeeQuery(Guid Id) : IRequest<Result<EmployeeDetailDto>>;

public sealed class GetEmployeeQueryHandler(
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<GetEmployeeQuery, Result<EmployeeDetailDto>>
{
    public async Task<Result<EmployeeDetailDto>> Handle(GetEmployeeQuery request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<EmployeeDetailDto>(Error.Forbidden("Organisation context is required."));

        var emp = await employees.GetByIdAsync(request.Id, currentUser.OrganisationId.Value, ct);
        if (emp is null)
            return Result.Failure<EmployeeDetailDto>(Error.NotFound("Employee", request.Id));

        var licences = emp.Licences.Select(l => new LicenceDto(
            l.Id, l.LicenceNumber, l.Category.ToString(), l.Subcategory,
            l.IssuingAuthority, l.IssuedAt, l.ExpiresAt, l.TypeRatings,
            l.ScopeNotes, l.AttachmentRef, l.IsExpired, l.IsCurrent)).ToList();

        var auths = emp.Authorisations.Select(a => new AuthorisationDto(
            a.Id, a.AuthorisationNumber, a.Category.ToString(), a.Scope,
            a.AircraftTypes, a.ComponentScope, a.StationScope, a.IssuingAuthority,
            a.ValidFrom, a.ValidUntil, a.IssuedByUserId,
            a.Status.ToString(), a.RevisionNumber, a.SuspensionReason,
            a.IsExpired, a.IsCurrent)).ToList();

        var training = emp.TrainingRecords.Select(t => new TrainingRecordDto(
            t.Id, t.CourseCode, t.CourseName, t.TrainingProvider,
            t.TrainingType.ToString(), t.CompletedAt, t.ExpiresAt,
            t.Result, t.CertificateRef, t.IsExpired, t.IsRecurring)).ToList();

        var assessments = emp.CompetencyAssessments
            .OrderByDescending(a => a.AssessmentDate)
            .Select(a => new CompetencyAssessmentDto(
                a.Id, a.AssessorId, a.AssessmentDate,
                a.AssessmentType.ToString(), a.Result.ToString(),
                a.Comments, a.NextReviewDate, a.IsCurrent, a.IsReviewOverdue))
            .ToList();

        return Result.Success(new EmployeeDetailDto(
            emp.Id, emp.EmployeeNumber, emp.FirstName, emp.LastName,
            emp.Email, emp.Phone, emp.Status.ToString(), emp.IsActive, emp.DateOfBirth,
            emp.NationalityCode, emp.UserId, emp.DefaultStationId,
            emp.EmergencyContactName, emp.EmergencyContactPhone,
            licences, auths, training, assessments, emp.CreatedAt));
    }
}
