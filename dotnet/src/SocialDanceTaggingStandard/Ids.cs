namespace SocialDanceTaggingStandard;

// Strongly-typed identifier kinds (CLAUDE.md coding guideline 1): each id KIND is its own type so the
// compiler — not a code review — rejects passing one where another is expected. No implicit string
// conversion (that would defeat the purpose); unwrap explicitly via .Value at the binding edge only.
// The literal VALUES are defined once, generated from YAML where a YAML source exists (guideline 2).

/// <summary>
/// A Social-Dance Tagging Standard field id — the stable, format-neutral identifier from
/// <c>data/fields.yaml</c> (<c>fields[].id</c>, e.g. <c>dances</c>, <c>genre</c>, <c>bpm</c>). It is the
/// lingua franca every binding reaches a field THROUGH; a <see cref="FieldId"/> is intentionally not
/// assignable where an <c>Id3FrameId</c>, a frame description, or a vocabulary code is expected.
/// </summary>
/// <param name="Value">The raw field id (lower-snake, matches <c>fields.yaml</c>).</param>
public readonly record struct FieldId(string Value)
{
    /// <summary>The raw field id string.</summary>
    public override string ToString() => Value;
}

/// <summary>
/// An opaque key for one value in a tag container — an ID3 <c>TXXX</c>/<c>COMM</c> description, a Vorbis
/// field name, an MP4 atom, or a sync-state slot. Format-neutral: the <c>Sync</c> engine moves these
/// without interpreting them (what a key means is the binding's concern). Its own type so a
/// <see cref="TagKey"/> can't be passed where a <see cref="FieldId"/> is expected, and vice-versa.
/// </summary>
/// <param name="Value">The raw container key (e.g. <c>SDTS_DANCES</c>, <c>Songs-DB_Occasion</c>).</param>
public readonly record struct TagKey(string Value)
{
    /// <summary>The raw container-key string.</summary>
    public override string ToString() => Value;
}
