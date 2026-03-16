using Mro.Domain.Aggregates.Inventory.Enums;
using Mro.Domain.Application;
using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Inventory;

/// <summary>
/// Catalogue record for a rotable/consumable part.
/// </summary>
public sealed class Part : AuditableEntity
{
    public string PartNumber { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string? AtaChapter { get; private set; }
    public string UnitOfMeasure { get; private set; } = null!;
    public string? Manufacturer { get; private set; }
    public string? ManufacturerPartNumber { get; private set; }
    public bool TraceabilityRequired { get; private set; }
    public decimal MinStockLevel { get; private set; }
    public PartStatus Status { get; private set; } = PartStatus.Active;

    private Part() { }

    public static Part Create(
        string partNumber,
        string description,
        string unitOfMeasure,
        Guid organisationId,
        Guid actorId,
        string? ataChapter = null,
        string? manufacturer = null,
        string? manufacturerPartNumber = null,
        bool traceabilityRequired = false,
        decimal minStockLevel = 0) => new()
    {
        PartNumber              = partNumber.ToUpperInvariant(),
        Description             = description,
        UnitOfMeasure           = unitOfMeasure,
        OrganisationId          = organisationId,
        AtaChapter              = ataChapter,
        Manufacturer            = manufacturer,
        ManufacturerPartNumber  = manufacturerPartNumber,
        TraceabilityRequired    = traceabilityRequired,
        MinStockLevel           = minStockLevel,
        CreatedAt               = DateTimeOffset.UtcNow,
        CreatedBy               = actorId,
    };

    public DomainResult Discontinue(Guid actorId)
    {
        if (Status == PartStatus.Obsolete)
            return DomainResult.Failure("Part is already obsolete.");
        Status    = PartStatus.Discontinued;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        return DomainResult.Ok();
    }

    public DomainResult MarkObsolete(Guid actorId)
    {
        Status    = PartStatus.Obsolete;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
        return DomainResult.Ok();
    }
}
