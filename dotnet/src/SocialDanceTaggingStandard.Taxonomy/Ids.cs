namespace SocialDanceTaggingStandard.Taxonomy;

// Strongly-typed vocabulary identifier kinds (CLAUDE.md coding guideline 1). The literal members are
// generated from data/vocabulary/ (guideline 2) into Generated/DimensionIds.g.cs and PackIds.g.cs.
// Per-term display codes are NOT a single shared kind: each dimension has its OWN code type
// (GenreCode, MoodCode, DanceCode, …) generated alongside its term catalog (Generated/Genre.g.cs, …),
// so a genre code can't be passed where a dance code is expected.

/// <summary>
/// A taxonomy dimension (axis) id — the key of a vocabulary dimension in
/// <see cref="SocialDanceVocabulary.Core"/> (e.g. <c>genre</c>, <c>mood</c>, <c>rhythmic_feel</c>).
/// </summary>
/// <param name="Value">The raw dimension id.</param>
public readonly record struct DimensionId(string Value)
{
    /// <summary>The raw dimension id string.</summary>
    public override string ToString() => Value;
}

/// <summary>
/// A dance-pack id — the key of a pack in <see cref="SocialDanceVocabulary.Packs"/> (e.g. <c>swing</c>,
/// <c>blues</c>, <c>tango</c>). Kept distinct from <see cref="DimensionId"/> (a pack is a family of dances,
/// not a per-track axis).
/// </summary>
/// <param name="Value">The raw pack id.</param>
public readonly record struct PackId(string Value)
{
    /// <summary>The raw pack id string.</summary>
    public override string ToString() => Value;
}
