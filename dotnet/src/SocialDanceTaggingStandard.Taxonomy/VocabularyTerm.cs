namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// A single canonical value in a taxonomy dimension — e.g. the genre <c>"Swing"</c> or the
/// dance <c>"Lindy Hop"</c> — together with its aliases and dimension-specific attributes.
/// </summary>
public sealed class VocabularyTerm
{
    /// <summary>Creates a vocabulary term.</summary>
    public VocabularyTerm(
        string id,
        string canonical,
        IReadOnlyList<string> aliases,
        bool deprecated,
        IReadOnlyDictionary<string, object?> attributes,
        TermDisplay? display = null)
    {
        Id = id;
        Canonical = canonical;
        Aliases = aliases;
        Deprecated = deprecated;
        Attributes = attributes;
        Display = display;
    }

    /// <summary>Stable kebab-case identifier, e.g. <c>"lindy-hop"</c>.</summary>
    public string Id { get; }

    /// <summary>Canonical value as stored on a track, e.g. <c>"Lindy Hop"</c>.</summary>
    public string Canonical { get; }

    /// <summary>Alternate forms that resolve to <see cref="Canonical"/>, e.g. <c>"#LH"</c>, <c>"Lindy"</c>
    /// (the bare code <c>"LH"</c> resolves via <see cref="TermDisplay.Code"/>, not as an alias).</summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>True if the value is retired; the successor (if any) is the <c>replaced_by</c> attribute.</summary>
    public bool Deprecated { get; }

    /// <summary>
    /// Dimension-specific metadata carried verbatim from the vocabulary, e.g. <c>family</c>,
    /// <c>tempo_min</c>/<c>tempo_max</c>, <c>energy</c>/<c>valence</c>, <c>parent</c>,
    /// <c>dance_relevance</c>, <c>typical_genres</c>. Scalars are strings; lists/maps are nested.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Attributes { get; }

    /// <summary>UI display metadata (code/emoji/icon/color), or null if none authored.</summary>
    public TermDisplay? Display { get; }

    /// <summary>The dance/genre family, if this dimension defines one (else null).</summary>
    public string? Family =>
        Attributes.TryGetValue("family", out var v) ? v as string : null;

    /// <summary>Returns a copy with a replaced alias list (used by customization overlays).</summary>
    public VocabularyTerm WithAliases(IReadOnlyList<string> aliases)
        => new VocabularyTerm(Id, Canonical, aliases, Deprecated, Attributes, Display);

    /// <summary>Returns a copy with replaced display metadata (used by personal-profile color/label
    /// overrides). Presentation only — the canonical (the hashed value) is unchanged.</summary>
    public VocabularyTerm WithDisplay(TermDisplay? display)
        => new VocabularyTerm(Id, Canonical, Aliases, Deprecated, Attributes, display);

    /// <inheritdoc/>
    public override string ToString() => Canonical;
}
