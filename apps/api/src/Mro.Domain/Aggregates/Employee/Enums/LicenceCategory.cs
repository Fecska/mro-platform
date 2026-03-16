namespace Mro.Domain.Aggregates.Employee.Enums;

/// <summary>
/// EASA Part-66 aircraft maintenance licence categories.
/// Reference: Commission Regulation (EU) No 1321/2014 Annex III (Part-66).
/// </summary>
public enum LicenceCategory
{
    /// <summary>
    /// Category A — Line maintenance certifying mechanic.
    /// Limited to routine scheduled line maintenance and simple defect rectification.
    /// </summary>
    A,

    /// <summary>
    /// Category B1 — Certifying staff for mechanical systems (airframe, engine, mechanical/avionics).
    /// Subcategories (B1.1–B1.4) stored in <see cref="Licence.Subcategory"/>.
    /// </summary>
    B1,

    /// <summary>
    /// Category B2 — Certifying staff for avionics/electrical systems.
    /// Subcategory B2L (Limited) stored in <see cref="Licence.Subcategory"/>.
    /// </summary>
    B2,

    /// <summary>
    /// Category B3 — Certifying staff for non-pressurised piston-engine aeroplanes ≤2000 kg MTOM.
    /// </summary>
    B3,

    /// <summary>
    /// Category C — Certifying staff for base maintenance (whole aircraft release).
    /// Requires B1 or B2 background per Part-66.66(e).
    /// </summary>
    C,
}
