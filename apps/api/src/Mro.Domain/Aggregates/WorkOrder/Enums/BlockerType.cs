namespace Mro.Domain.Aggregates.WorkOrder.Enums;

/// <summary>
/// Classifies why a Work Order is blocked.
/// Determines which queue the WO appears in on the planning board.
/// </summary>
public enum BlockerType
{
    /// <summary>Awaiting delivery or issue of a required part from stores.</summary>
    WaitingParts,

    /// <summary>Awaiting availability of a specific calibrated tool or GSE.</summary>
    WaitingTooling,

    /// <summary>Awaiting an engineering disposition or design concession.</summary>
    WaitingEngineering,

    /// <summary>Awaiting management or regulatory approval before proceeding.</summary>
    WaitingApproval,

    /// <summary>External factor outside the MRO's control (weather, slot, customer).</summary>
    ExternalFactor,
}
