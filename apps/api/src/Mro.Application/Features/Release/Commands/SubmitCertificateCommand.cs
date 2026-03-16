using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;

namespace Mro.Application.Features.Release.Commands;

public sealed class SubmitCertificateCommand : IRequest<Result<Unit>>
{
    public required Guid CertificateId { get; init; }
}

public sealed class SubmitCertificateCommandHandler(
    IReleaseCertificateRepository certificates,
    ICurrentUserService currentUser)
    : IRequestHandler<SubmitCertificateCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SubmitCertificateCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Unit>(Error.Forbidden("Organisation context is required."));

        var cert = await certificates.GetByIdAsync(
            request.CertificateId, currentUser.OrganisationId.Value, ct);
        if (cert is null)
            return Result.Failure<Unit>(Error.NotFound("ReleaseCertificate", request.CertificateId));

        var result = cert.Submit(currentUser.UserId!.Value);
        if (result.IsFailure)
            return Result.Failure<Unit>(Error.Validation(result.ErrorMessage!));

        await certificates.UpdateAsync(cert, ct);
        return Result.Success(Unit.Value);
    }
}
