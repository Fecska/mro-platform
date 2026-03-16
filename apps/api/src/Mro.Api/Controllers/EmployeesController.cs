using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mro.Application.Features.Employees.Commands;
using Mro.Application.Features.Employees.Queries;
using Mro.Domain.Aggregates.Employee.Enums;
using Mro.Application.Features.Employees.Dtos;
using AttachmentType = Mro.Domain.Aggregates.Employee.Enums.AttachmentType;
using RestrictionType = Mro.Domain.Aggregates.Employee.Enums.RestrictionType;

namespace Mro.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public sealed class EmployeesController(ISender sender) : ControllerBase
{
    // ── GET /api/employees ─────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] EmployeeStatus? status,
        [FromQuery] bool? isActive,
        [FromQuery] LicenceCategory? licenceCategory,
        [FromQuery] Guid? stationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new ListEmployeesQuery(status, isActive, licenceCategory, stationId, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/employees/expiring-credentials ────────────────────────────

    [HttpGet("expiring-credentials")]
    public async Task<IActionResult> ExpiringCredentials(
        [FromQuery] int days = 30,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetExpiringCredentialsQuery(days), ct);
        return result.IsSuccess ? Ok(result.Value) : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/employees/{id} ────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetEmployeeQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/employees ────────────────────────────────────────────────

    public sealed record CreateEmployeeRequest(
        string FirstName,
        string LastName,
        string Email,
        DateOnly DateOfBirth,
        Guid? UserId,
        string? Phone,
        string? NationalityCode,
        Guid? DefaultStationId,
        string? EmergencyContactName,
        string? EmergencyContactPhone);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new CreateEmployeeCommand
        {
            FirstName             = request.FirstName,
            LastName              = request.LastName,
            Email                 = request.Email,
            DateOfBirth           = request.DateOfBirth,
            UserId                = request.UserId,
            Phone                 = request.Phone,
            NationalityCode       = request.NationalityCode,
            DefaultStationId      = request.DefaultStationId,
            EmergencyContactName  = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
        }, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value }, new { id = result.Value })
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── PATCH /api/employees/{id} ──────────────────────────────────────────

    public sealed record UpdateEmployeeRequest(
        string? FirstName,
        string? LastName,
        string? Email,
        string? Phone,
        Guid? DefaultStationId,
        bool ClearDefaultStation,
        string? NationalityCode,
        string? EmergencyContactName,
        string? EmergencyContactPhone);

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new UpdateEmployeeCommand
        {
            EmployeeId            = id,
            FirstName             = request.FirstName,
            LastName              = request.LastName,
            Email                 = request.Email,
            Phone                 = request.Phone,
            DefaultStationId      = request.DefaultStationId,
            ClearDefaultStation   = request.ClearDefaultStation,
            NationalityCode       = request.NationalityCode,
            EmergencyContactName  = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
        }, ct);

        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/employees/{id}/licences ──────────────────────────────────

    [HttpGet("{id:guid}/licences")]
    public async Task<IActionResult> ListLicences(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetEmployeeLicencesQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/employees/{id}/authorisations ────────────────────────────

    [HttpGet("{id:guid}/authorisations")]
    public async Task<IActionResult> ListAuthorisations(
        Guid id,
        [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetEmployeeAuthorisationsQuery(id, activeOnly), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/employees/{id}/status ───────────────────────────────────

    public sealed record SetStatusRequest(EmployeeStatus NewStatus);

    [HttpPost("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] SetStatusRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new SetEmployeeStatusCommand
        {
            EmployeeId = id,
            NewStatus  = request.NewStatus,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/employees/{id}/licences ─────────────────────────────────

    public sealed record AddLicenceRequest(
        string LicenceNumber,
        LicenceCategory Category,
        string IssuingAuthority,
        DateOnly IssuedAt,
        string? Subcategory,
        DateOnly? ExpiresAt,
        string? TypeRatings,
        string? ScopeNotes,
        string? AttachmentRef);

    [HttpPost("{id:guid}/licences")]
    public async Task<IActionResult> AddLicence(Guid id, [FromBody] AddLicenceRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AddLicenceCommand
        {
            EmployeeId       = id,
            LicenceNumber    = request.LicenceNumber,
            Category         = request.Category,
            IssuingAuthority = request.IssuingAuthority,
            IssuedAt         = request.IssuedAt,
            Subcategory      = request.Subcategory,
            ExpiresAt        = request.ExpiresAt,
            TypeRatings      = request.TypeRatings,
            ScopeNotes       = request.ScopeNotes,
            AttachmentRef    = request.AttachmentRef,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/employees/{id}/authorisations ───────────────────────────

    public sealed record GrantAuthorisationRequest(
        string AuthorisationNumber,
        LicenceCategory Category,
        string Scope,
        Guid IssuingLicenceId,
        Guid IssuedByUserId,
        DateOnly ValidFrom,
        string? AircraftTypes,
        DateOnly? ValidUntil,
        string? ComponentScope,
        string? StationScope,
        string? IssuingAuthority);

    [HttpPost("{id:guid}/authorisations")]
    public async Task<IActionResult> GrantAuthorisation(Guid id, [FromBody] GrantAuthorisationRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new GrantAuthorisationCommand
        {
            EmployeeId          = id,
            AuthorisationNumber = request.AuthorisationNumber,
            Category            = request.Category,
            Scope               = request.Scope,
            IssuingLicenceId    = request.IssuingLicenceId,
            IssuedByUserId      = request.IssuedByUserId,
            ValidFrom           = request.ValidFrom,
            AircraftTypes       = request.AircraftTypes,
            ValidUntil          = request.ValidUntil,
            ComponentScope      = request.ComponentScope,
            StationScope        = request.StationScope,
            IssuingAuthority    = request.IssuingAuthority,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── DELETE /api/employees/{id}/authorisations/{authId} ────────────────

    public sealed record RevokeAuthorisationRequest(string Reason);

    [HttpDelete("{id:guid}/authorisations/{authId:guid}")]
    public async Task<IActionResult> RevokeAuthorisation(
        Guid id, Guid authId,
        [FromBody] RevokeAuthorisationRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new RevokeAuthorisationCommand
        {
            EmployeeId      = id,
            AuthorisationId = authId,
            Reason          = request.Reason,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/employees/{id}/training-records ──────────────────────────

    [HttpGet("{id:guid}/training-records")]
    public async Task<IActionResult> ListTrainingRecords(
        Guid id,
        [FromQuery] TrainingType? trainingType = null,
        [FromQuery] bool expiredOnly = false,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetEmployeeTrainingRecordsQuery(id, trainingType, expiredOnly), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/employees/{id}/training-records ─────────────────────────

    public sealed record RecordTrainingRequest(
        string CourseCode,
        string CourseName,
        string TrainingProvider,
        TrainingType TrainingType,
        DateOnly CompletedAt,
        DateOnly? ExpiresAt,
        string? Result,
        string? CertificateRef);

    [HttpPost("{id:guid}/training-records")]
    [HttpPost("{id:guid}/training")]
    public async Task<IActionResult> RecordTraining(Guid id, [FromBody] RecordTrainingRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RecordTrainingCommand
        {
            EmployeeId       = id,
            CourseCode       = request.CourseCode,
            CourseName       = request.CourseName,
            TrainingProvider = request.TrainingProvider,
            TrainingType     = request.TrainingType,
            CompletedAt      = request.CompletedAt,
            ExpiresAt        = request.ExpiresAt,
            Result           = request.Result,
            CertificateRef   = request.CertificateRef,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/employees/{id}/shifts ───────────────────────────────────

    public sealed record RecordShiftRequest(
        DateOnly ShiftDate,
        TimeOnly StartTime,
        TimeOnly EndTime,
        ShiftType ShiftType,
        AvailabilityStatus AvailabilityStatus,
        bool IsActual,
        Guid? StationId,
        string? Notes);

    [HttpPost("{id:guid}/shifts")]
    public async Task<IActionResult> RecordShift(Guid id, [FromBody] RecordShiftRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new RecordShiftCommand
        {
            EmployeeId         = id,
            ShiftDate          = request.ShiftDate,
            StartTime          = request.StartTime,
            EndTime            = request.EndTime,
            ShiftType          = request.ShiftType,
            AvailabilityStatus = request.AvailabilityStatus,
            IsActual           = request.IsActual,
            StationId          = request.StationId,
            Notes              = request.Notes,
        }, ct);

        return result.IsSuccess ? NoContent() : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/employees/{id}/currency ──────────────────────────────────

    [HttpGet("{id:guid}/currency")]
    public async Task<IActionResult> Currency(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetEmployeeCurrencyQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/employees/{id}/currency-status ────────────────────────────

    [HttpGet("{id:guid}/currency-status")]
    public async Task<IActionResult> CurrencyStatus(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetEmployeeCurrencyStatusQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/employees/{id}/attachments ────────────────────────────────

    [HttpGet("{id:guid}/attachments")]
    public async Task<IActionResult> ListAttachments(
        Guid id,
        [FromQuery] AttachmentType? type = null,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListEmployeeAttachmentsQuery
        {
            EmployeeId = id,
            Type       = type,
        }, ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/employees/{id}/attachments/upload-url ───────────────────

    public sealed record GetAttachmentUploadUrlRequest(string FileName, string ContentType);

    [HttpPost("{id:guid}/attachments/upload-url")]
    public async Task<IActionResult> GetAttachmentUploadUrl(
        Guid id,
        [FromBody] GetAttachmentUploadUrlRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetEmployeeAttachmentUploadUrlQuery
        {
            EmployeeId  = id,
            FileName    = request.FileName,
            ContentType = request.ContentType,
        }, ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/employees/{id}/attachments ──────────────────────────────

    public sealed record RegisterAttachmentRequest(
        AttachmentType AttachmentType,
        string DisplayName,
        string StoragePath,
        long FileSizeBytes,
        string ContentType,
        Guid? LinkedEntityId);

    [HttpPost("{id:guid}/attachments")]
    public async Task<IActionResult> RegisterAttachment(
        Guid id,
        [FromBody] RegisterAttachmentRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new RegisterEmployeeAttachmentCommand
        {
            EmployeeId     = id,
            AttachmentType = request.AttachmentType,
            DisplayName    = request.DisplayName,
            StoragePath    = request.StoragePath,
            FileSizeBytes  = request.FileSizeBytes,
            ContentType    = request.ContentType,
            LinkedEntityId = request.LinkedEntityId,
        }, ct);
        return result.IsSuccess ? Ok(new { id = result.Value })
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── DELETE /api/employees/{id}/attachments/{attachmentId} ─────────────

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DeleteAttachment(
        Guid id, Guid attachmentId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteEmployeeAttachmentCommand
        {
            EmployeeId   = id,
            AttachmentId = attachmentId,
        }, ct);
        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/employees/{id}/restrictions ───────────────────────────────

    [HttpGet("{id:guid}/restrictions")]
    public async Task<IActionResult> ListRestrictions(
        Guid id,
        [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new ListEmployeeRestrictionsQuery
        {
            EmployeeId = id,
            ActiveOnly = activeOnly,
        }, ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/employees/{id}/restrictions ──────────────────────────────

    public sealed record AddRestrictionRequest(
        RestrictionType RestrictionType,
        Guid RaisedByUserId,
        DateOnly ActiveFrom,
        string? Details,
        Guid? StationId,
        DateOnly? ActiveUntil);

    [HttpPost("{id:guid}/restrictions")]
    public async Task<IActionResult> AddRestriction(
        Guid id, [FromBody] AddRestrictionRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AddEmployeeRestrictionCommand
        {
            EmployeeId       = id,
            RestrictionType  = request.RestrictionType,
            RaisedByUserId   = request.RaisedByUserId,
            ActiveFrom       = request.ActiveFrom,
            Details          = request.Details,
            StationId        = request.StationId,
            ActiveUntil      = request.ActiveUntil,
        }, ct);
        return result.IsSuccess ? Ok(new { id = result.Value })
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── DELETE /api/employees/{id}/restrictions/{restrictionId} ───────────

    [HttpDelete("{id:guid}/restrictions/{restrictionId:guid}")]
    public async Task<IActionResult> LiftRestriction(
        Guid id, Guid restrictionId, CancellationToken ct)
    {
        var result = await sender.Send(new LiftEmployeeRestrictionCommand
        {
            EmployeeId    = id,
            RestrictionId = restrictionId,
        }, ct);
        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/employees/{id}/notes ──────────────────────────────────────

    [HttpGet("{id:guid}/notes")]
    public async Task<IActionResult> ListNotes(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new ListEmployeeNotesQuery { EmployeeId = id }, ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── POST /api/employees/{id}/notes ─────────────────────────────────────

    public sealed record AddNoteRequest(string NoteText, bool IsConfidential);

    [HttpPost("{id:guid}/notes")]
    public async Task<IActionResult> AddNote(
        Guid id, [FromBody] AddNoteRequest request, CancellationToken ct)
    {
        var result = await sender.Send(new AddEmployeeNoteCommand
        {
            EmployeeId     = id,
            NoteText       = request.NoteText,
            IsConfidential = request.IsConfidential,
        }, ct);
        return result.IsSuccess ? Ok(new { id = result.Value })
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── DELETE /api/employees/{id}/notes/{noteId} ──────────────────────────

    [HttpDelete("{id:guid}/notes/{noteId:guid}")]
    public async Task<IActionResult> DeleteNote(Guid id, Guid noteId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteEmployeeNoteCommand
        {
            EmployeeId = id,
            NoteId     = noteId,
        }, ct);
        return result.IsSuccess ? NoContent()
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }

    // ── GET /api/employees/{id}/timeline ───────────────────────────────────

    [HttpGet("{id:guid}/timeline")]
    public async Task<IActionResult> Timeline(
        Guid id,
        [FromQuery] int daysBack = 90,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetEmployeeTimelineQuery
        {
            EmployeeId = id,
            DaysBack   = daysBack,
            Limit      = limit,
        }, ct);
        return result.IsSuccess ? Ok(result.Value)
            : result.Error.Code == "NOT_FOUND" ? NotFound(result.Error.Message)
            : Problem(result.Error.Message, statusCode: 400);
    }
}
