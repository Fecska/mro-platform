using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Defect;
using Mro.Domain.Aggregates.Defect.Enums;

namespace Mro.Application.Features.Defects.Commands;

public sealed class RaiseDefectCommand : IRequest<Result<Guid>>
{
    public required Guid AircraftId { get; init; }
    public required DefectSeverity Severity { get; init; }
    public required DefectSource Source { get; init; }
    public required string AtaChapter { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset DiscoveredAt { get; init; }
    public Guid? DiscoveredAtStationId { get; init; }
    public bool IsAdMandated { get; init; }
    public Guid? AdDocumentId { get; init; }
}

public sealed class RaiseDefectCommandHandler(
    IDefectRepository defects,
    ICurrentUserService currentUser)
    : IRequestHandler<RaiseDefectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RaiseDefectCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var orgId = currentUser.OrganisationId.Value;
        var actorId = currentUser.UserId!.Value;

        var count = await defects.CountAsync(orgId, null, null, null, ct);
        var defectNumber = $"DEF-{DateTimeOffset.UtcNow.Year}-{(count + 1):D5}";

        Defect defect;
        try
        {
            defect = Defect.Raise(
                defectNumber,
                request.AircraftId,
                request.Severity,
                request.Source,
                request.AtaChapter,
                request.Description,
                request.DiscoveredAt,
                orgId,
                actorId,
                request.DiscoveredAtStationId,
                request.IsAdMandated,
                request.AdDocumentId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(Error.Validation(ex.Message));
        }

        await defects.AddAsync(defect, ct);
        return Result.Success(defect.Id);
    }
}
