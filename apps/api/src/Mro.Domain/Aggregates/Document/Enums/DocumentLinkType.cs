namespace Mro.Domain.Aggregates.Document.Enums;

/// <summary>
/// Characterises the relationship between a task and a document.
/// Determines whether the document must be consulted before task sign-off.
/// </summary>
public enum DocumentLinkType
{
    /// <summary>
    /// Mandatory — technician must confirm document has been consulted.
    /// Sign-off is blocked if current revision is not acknowledged (HS-007).
    /// </summary>
    Mandatory,

    /// <summary>Document provides additional guidance; consultation is recommended but not enforced.</summary>
    Reference,

    /// <summary>Informational only — attached for context, no compliance gate.</summary>
    Informational,
}
