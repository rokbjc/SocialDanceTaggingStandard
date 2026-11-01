using SocialDanceTaggingStandard.Sync;

namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// The taxonomy-backed <see cref="IValueTranslator"/> — the seam where value-level migration plugs
/// into the <c>Sync</c> engine. Resolves each (multi-)value through the vocabulary:
/// <list type="bullet">
///   <item><description><see cref="MirrorToCanonical"/>: aliases / legacy <c>#</c>-codes → canonical
///   (e.g. dances <c>"#LH"</c> → <c>"Lindy Hop"</c>, genre <c>"Swing Era"</c> → <c>"Swing"</c>).</description></item>
///   <item><description><see cref="CanonicalToMirror"/>: canonical → mirror form. For <c>dances</c> the
///   mirror is the <c>#</c>-code (MediaMonkey Occasion); other fields mirror the canonical name.</description></item>
/// </list>
/// Unresolvable tokens pass through verbatim (never lose data); unknown fields pass through whole.
/// Pass it to a binding's <c>translator</c> parameter (e.g. <c>MediaMonkeyMirror.Open(file, translator: t)</c>).
/// </summary>
public sealed class TaxonomyValueTranslator : IValueTranslator
{
    private readonly SocialDanceVocabulary _vocab;
    private readonly string _delimiter;
    private readonly Dimension _dances;

    /// <summary>Creates a translator over the vocabulary. <paramref name="enabledPacks"/> selects which
    /// dance packs back the <c>dances</c> field (default: all packs). <paramref name="delimiter"/> is the
    /// multi-value separator (default <c>"; "</c>).</summary>
    public TaxonomyValueTranslator(SocialDanceVocabulary vocab, string delimiter = "; ", params PackId[] enabledPacks)
    {
        _vocab = vocab ?? throw new ArgumentNullException(nameof(vocab));
        _delimiter = string.IsNullOrEmpty(delimiter) ? "; " : delimiter;
        var packs = enabledPacks is { Length: > 0 } ? enabledPacks : [.. vocab.Packs.Keys];
        _dances = vocab.Dances(packs);
    }

    /// <inheritdoc />
    public string? MirrorToCanonical(FieldId field, string? mirrorValue) => Map(field, mirrorValue, toMirror: false);

    /// <inheritdoc />
    public string? CanonicalToMirror(FieldId field, string? canonicalValue) => Map(field, canonicalValue, toMirror: true);

    // The vocabulary dimension that backs a field; a field id maps to the same-named dimension, and the
    // 'dances' field to the combined dances dimension over the enabled packs.
    private Dimension? DimensionFor(DimensionId dimension)
        => dimension == SocialDanceVocabulary.DancesDimension ? _dances : _vocab.Dimension(dimension);

    // The vocabulary dimension a field draws from. It comes from the generated FieldCatalog — the same
    // data/fields.yaml that names the two deliberate splits (`dance` binds `dances`, the cross-pack union;
    // `instrument_led` binds the unified `instrument` dimension) — so this mapping has ONE source, not a
    // ternary here plus a table there. A field with no vocabulary backing (a number, a cue) falls to its
    // own id, which DimensionFor then resolves to null.
    private static DimensionId BackingDimension(FieldId field)
        => FieldCatalog.For(field)?.BackingDimension is { } dim
            ? new DimensionId(dim)
            : new DimensionId(field.Value);

    private string? Map(FieldId field, string? value, bool toMirror)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        var dim = DimensionFor(BackingDimension(field));
        if (dim is null)
            return value; // unknown field → pass through
        var parts = value.Split([_delimiter], StringSplitOptions.None);
        return string.Join(_delimiter, parts.Select(p => MapToken(field, dim, p.Trim(), toMirror)));
    }

    private static string MapToken(FieldId field, Dimension dim, string token, bool toMirror)
    {
        if (token.Length == 0) return token;
        var term = dim.Resolve(token);
        if (term is null) return token; // unresolvable → keep verbatim
        if (!toMirror) return term.Canonical;
        if (field != FieldIds.Dance) return term.Canonical;
        // The dance mirror carries the term's DISPLAY CODE (LH, BAL, SLS): MediaMonkey's Occasion
        // column is scanned at a glance, where a canonical name is too long to be useful.
        //
        // But a code is only safe to WRITE if it reads back as the same term IN THIS vocabulary. An
        // overlay can bind a code to a different term — a DJ who never dances St. Louis Shag may map
        // bare 'SLS' to Salsa, and codes are gap-filled, so the overlay's alias wins. Emitting 'SLS'
        // for St. Louis Shag would then be read back as Salsa: silently the wrong dance, with a
        // correct overlay and a correct baseline. So the code is verified against the resolver it
        // will be read by, and the canonical name (which always resolves to itself) is the fallback.
        var code = term.Display?.Code;
        if (!string.IsNullOrEmpty(code) && dim.Resolve(code)?.Id == term.Id)
            return code;
        return term.Canonical;
    }
}
