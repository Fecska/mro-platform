using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Release.Commands;

public sealed class VoidCertificateCommand : IRequest<Result<Unit>>
{
    public required Guid CertificateId { get; init; }
    public required string Reason { get; init; }
}

public sealed class VoidCertificateCommandHandler(
    IReleaseCertificateRepository certificates,
    ICurrentUserService currentUser)
    : IRequestHandler<VoidCertificateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(VoidCertificateCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var cert = await certificates.GetByIdAsync(
            request.CertificateId, currentUser.OrganisationId.Value, ct);
        if (cert is null)
            return Result.Failure<Unit>(Error.NotFound("ReleaseCertificate", request.CertificateId));

        var result = cert.Void(request.Reason, currentUser.UserId!.Value);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await certificates.UpdateAsync(cert, ct);
        return Result.Success(Unit.Value);
    }
}
