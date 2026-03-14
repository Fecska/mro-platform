using Mro.Domain.Entities;

namespace Mro.Domain.Aggregates.Document;

/// <summary>
/// Defines the applicability of a document to a specific aircraft type
/// and optionally to a serial number range.
///
/// Examples:
///   - IcaoTypeCode="B738", SerialFrom=null, SerialTo=null → applies to all B738
///   - IcaoTypeCode="A320", SerialFrom="1234", SerialTo="1500" → applies to A320 MSN 1234–1500
///   - IcaoTypeCode=null → applies to all aircraft types (e.g. a cross-fleet AD)
///
/// A document may have multiple effectivity records for different type/serial combinations.
/// </summary>
public sealed class DocumentEffectivity : AuditableEntity
{
    public Guid DocumentId { get; private set; }

    /// <summary>ICAO type designator. Null means all types.</summary>
    public string? IcaoTypeCode { get; private set; }

    /// <summary>
    /// Manufacturer serial number (MSN) range — inclusive lower bound.
    /// Null means from the very first serial.
    /// </summary>
    public string? SerialFrom { get; private set; }

    /// <summary>
    /// MSN range — inclusive upper bound.
    /// Null means through the last serial (open-ended).
    /// </summary>
    public string? SerialTo { get; private set; }

    /// <summary>
    /// Modification status or other condition text as printed in the document
    /// (e.g. "Post SB-737-27-0123", "Pre-modification").
    /// </summary>
    public string? ConditionNote { get; private set; }

    // EF Core
    private DocumentEffectivity() { }

    internal static DocumentEffectivity Create(
        Guid documentId,
        string? icaoTypeCode,
        string? serialFrom,
        string? serialTo,
        string? conditionNote,
        Guid organisationId,
        Guid actorId)
    {
        return new DocumentEffectivity
        {
            DocumentId = documentId,
            IcaoTypeCode = icaoTypeCode?.Trim().ToUpperInvariant(),
            SerialFrom = serialFrom?.Trim().ToUpperInvariant(),
            SerialTo = serialTo?.Trim().ToUpperInvariant(),
            ConditionNote = conditionNote?.Trim(),
            OrganisationId = organisationId,
            CreatedBy = actorId,
            UpdatedBy = actorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Returns true if this effectivity covers the specified aircraft type and serial number.
    /// </summary>
    public bool Covers(string icaoTypeCode, string serialNumber)
    {
        if (IcaoTypeCode is not null &&
            !IcaoTypeCode.Equals(icaoTypeCode, StringComparison.OrdinalIgnoreCase))
            return false;

        // Serial number range check (lexicographic — adequate for MSN numeric codes)
        if (SerialFrom is not null &&
            string.Compare(serialNumber, SerialFrom, StringComparison.OrdinalIgnoreCase) < 0)
            return false;

        if (SerialTo is not null &&
            string.Compare(serialNumber, SerialTo, StringComparison.OrdinalIgnoreCase) > 0)
            return false;

        return true;
    }
}
