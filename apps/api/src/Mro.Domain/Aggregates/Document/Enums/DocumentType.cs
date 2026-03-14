namespace Mro.Domain.Aggregates.Document.Enums;

/// <summary>
/// Classification of a maintenance document by its technical purpose.
/// Stored as string in the database.
/// Regulatory references follow ATA iSpec 2200 chapter structure.
/// </summary>
public enum DocumentType
{
    /// <summary>Aircraft Maintenance Manual — primary maintenance instructions.</summary>
    Amm,

    /// <summary>Illustrated Parts Catalog.</summary>
    Ipc,

    /// <summary>Wiring Diagram Manual.</summary>
    Wdm,

    /// <summary>Structural Repair Manual.</summary>
    Srm,

    /// <summary>Component Maintenance Manual (for a specific LRU).</summary>
    Cmm,

    /// <summary>Service Bulletin — manufacturer-issued modification or inspection instruction.</summary>
    ServiceBulletin,

    /// <summary>
    /// Airworthiness Directive — mandatory regulatory instruction.
    /// Compliance is a hard stop for release (HS-006).
    /// </summary>
    AirworthinessDirective,

    /// <summary>Master Minimum Equipment List.</summary>
    Mmel,

    /// <summary>
    /// Repair / Modification Order — internal engineering order.
    /// Organisation-specific; not from a type certificate holder.
    /// </summary>
    EngineeringOrder,

    /// <summary>Non-destructive Testing Manual.</summary>
    NdtManual,

    /// <summary>Tool and Equipment Manual.</summary>
    ToolManual,

    /// <summary>Any other technical document not covered by the categories above.</summary>
    Other,
}
