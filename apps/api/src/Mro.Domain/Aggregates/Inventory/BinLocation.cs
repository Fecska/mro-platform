using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Inventory;

/// <summary>Physical bin/shelf location in a store room.</summary>
public sealed class BinLocation : AuditableEntity
{
    public string Code { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? StoreRoom { get; private set; }
    public bool IsActive { get; private set; } = true;

    private BinLocation() { }

    public static BinLocation Create(
        string code,
        Guid organisationId,
        Guid actorId,
        string? description = null,
        string? storeRoom = null) => new()
    {
        Code           = code.ToUpperInvariant(),
        Description    = description,
        StoreRoom      = storeRoom,
        OrganisationId = organisationId,
        CreatedAt      = DateTimeOffset.UtcNow,
        CreatedBy      = actorId,
    };

    public void Deactivate(Guid actorId)
    {
        IsActive  = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorId;
    }
}
