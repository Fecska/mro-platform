using MediatR;
using Mro.Application.Abstractions;
using Mro.Application.Common;
using Mro.Domain.Aggregates.Maintenance;

namespace Mro.Application.Features.Maintenance.Commands;

public sealed class CreateMaintenanceProgramCommand : IRequest<Result<Guid>>
{
    public required string ProgramNumber { get; init; }
    public required string AircraftTypeCode { get; init; }
    public required string Title { get; init; }
    public required string RevisionNumber { get; init; }
    public required DateOnly RevisionDate { get; init; }
    public string? ApprovalReference { get; init; }
}

public sealed class CreateMaintenanceProgramCommandHandler(
    IMaintenanceProgramRepository programs,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateMaintenanceProgramCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMaintenanceProgramCommand request, CancellationToken ct)
    {
        if (currentUser.OrganisationId is null)
            return Result.Failure<Guid>(Error.Forbidden("Organisation context is required."));

        var program = MaintenanceProgram.Create(
            request.ProgramNumber, request.AircraftTypeCode, request.Title,
            request.RevisionNumber, request.RevisionDate,
            currentUser.OrganisationId.Value, currentUser.UserId!.Value,
            request.ApprovalReference);

        await programs.AddAsync(program, ct);
        return Result.Success(program.Id);
    }
}
