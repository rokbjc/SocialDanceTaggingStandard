namespace SocialDanceTaggingStandard.Gates;

/// <summary>
/// One confirmation-gate entry: the pipeline version it was stamped at, plus an optional CRC of the gated
/// field's normalized content at confirm time. The CRC lets a later load detect that *someone else* (MM5,
/// another DJ, a sync tool) changed the field after we confirmed it — a change we didn't make ourselves, so
/// <c>GateInvalidator</c> never cleared the gate. A null CRC means "no integrity stamp" (an older
/// gate written before CRCs, or a hand-edit) and is treated as intact, never as tampered.
/// </summary>
/// <param name="Version">Pipeline version the gate was stamped at (positive).</param>
/// <param name="Crc">Lowercase 4-hex-digit content stamp (low 16 bits of CRC32), or null if unstamped.</param>
public sealed record GateEntry(int Version, string? Crc);
