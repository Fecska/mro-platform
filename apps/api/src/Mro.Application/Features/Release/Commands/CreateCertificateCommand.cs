using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Release;
using Mro.Domain.Aggregates.Release.Enums;

namespace Mro.Application.Features.Release.Commands;

public sealed class CreateCertificateCommand : IRequest<Result<Guid>>
{
    public required CertificateType CertificateType { get; init; }
    public required Guid WorkOrderId { get; init; }
    public required Guid AircraftId { get; init; }
    public required string AircraftRegistration { get; init; }
    public required string WorkOrderNumber { get; init; }
    public required string Scope { get; init; }
    public required string RegulatoryBasis { get; init; }
    public required Guid CertifyingStaffUserId { get; init; }
    public string? LimitationsAndRemarks { get; init; }
}

public sealed class CreateCertificateCommandHandler(
    IReleaseCertificateRepository certificates,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateCertificateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateCertificateCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId   = currentUser.OrganisationId.Value;
        var actorId = currentUser.UserId!.Value;

        var count  = await certificates.CountAsync(orgId, ct);
        var prefix = request.CertificateType == CertificateType.Form1 ? "FORM1" : "CRS";
        var number = $"{prefix}-{DateTimeOffset.UtcNow.Year}-{(count + 1):D5}";

        var cert = ReleaseCertificate.Create(
            number, request.CertificateType,
            request.WorkOrderId, request.AircraftId,
            request.AircraftRegistration, request.WorkOrderNumber,
            request.Scope, request.RegulatoryBasis,
            request.CertifyingStaffUserId,
            orgId, actorId,
            request.LimitationsAndRemarks);

        await certificates.AddAsync(cert, ct);
        return Result.Success(cert.Id);
    }
}
