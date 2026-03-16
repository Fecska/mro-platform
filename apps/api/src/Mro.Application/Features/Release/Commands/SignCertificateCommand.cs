using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Release.Enums;

namespace Mro.Application.Features.Release.Commands;

public sealed class SignCertificateCommand : IRequest<Result<Unit>>
{
    public required Guid CertificateId { get; init; }
    public required Guid SignerUserId { get; init; }
    public required string LicenceRef { get; init; }
    public required SignatureMethod Method { get; init; }
    public required string StatementText { get; init; }
    public string? IpAddress { get; init; }
}

public sealed class SignCertificateCommandHandler(
    IReleaseCertificateRepository certificates,
    IEmployeeRepository employees,
    ICurrentUserService currentUser)
    : IRequestHandler<SignCertificateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SignCertificateCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;

        // ── HS-001 / HS-014: Certifying engineer compliance checks ───────────
        var signer = await employees.GetByUserIdAsync(request.SignerUserId, orgId, ct);
        if (signer is not null)
        {
            // HS-001: Must have at least one current (non-expired, non-suspended) authorisation
            var currentAuths = signer.Authorisations.Where(a => a.IsCurrent).ToList();
            if (currentAuths.Count == 0)
            {
                var detail = signer.Authorisations.Any()
                    ? string.Join(", ", signer.Authorisations
                        .Where(a => a.IsActive)
                        .Select(a => a.IsSuspended
                            ? $"'{a.AuthorisationNumber}' (suspended)"
                            : $"'{a.AuthorisationNumber}' (expired {a.ValidUntil:yyyy-MM-dd})"))
                    : "no authorisations on record";
                return Result.Failure<Unit>(Error.HardStop("HS-001",
                    $"CRS signing blocked: certifying engineer has no current authorisation — {detail}."));
            }

            // HS-014: All recurring training must be current
            var expiredTraining = signer.TrainingRecords
                .Where(t => t.IsRecurring && t.IsExpired)
                .ToList();

            if (expiredTraining.Count > 0)
            {
                var courses = string.Join(", ",
                    expiredTraining.Select(t => $"'{t.CourseCode}' (expired {t.ExpiresAt:yyyy-MM-dd})"));
                return Result.Failure<Unit>(Error.HardStop("HS-014",
                    $"CRS signing blocked: certifying engineer has expired recurrent training. " +
                    $"Courses requiring renewal: {courses}."));
            }
        }

        var cert = await certificates.GetByIdAsync(request.CertificateId, orgId, ct);
        if (cert is null)
            return Result.Failure<Unit>(Error.NotFound("ReleaseCertificate", request.CertificateId));

        var result = cert.Sign(
            request.SignerUserId, request.LicenceRef, request.Method, request.StatementText,
            currentUser.UserId!.Value, request.IpAddress);

        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await certificates.UpdateAsync(cert, ct);
        return Result.Success(Unit.Value);
    }
}
