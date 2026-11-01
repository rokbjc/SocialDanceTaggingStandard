namespace SocialDanceTaggingStandard.Sync;

/// <summary>
/// One structural mapping between a canonical SDTS field and its key(s) in an external mirror —
/// for example, the <c>dances</c> field's <c>SDTS_DANCES</c> tag and the MediaMonkey
/// <c>COMM:Songs-DB_Occasion</c> frame. A field may have MORE THAN ONE mirror key (e.g. a legacy
/// duplicate such as <c>MusicMatch_Situation</c>): the FIRST key is authoritative for reads and
/// change-detection; all keys receive writes on export.
/// <para>
/// This is a <b>key correspondence only</b>, never a value mapping: value-level translation
/// (when a taxonomy exists) lives behind <see cref="IValueTranslator"/>.
/// </para>
/// </summary>
public sealed record FieldMirror
{
    /// <summary>Stable SDTS field id (e.g. <c>dances</c>) — the sync-state key.</summary>
    public FieldId Field { get; }

    /// <summary>Key in the canonical source (e.g. the TXXX description <c>SDTS_DANCES</c>).</summary>
    public TagKey CanonicalKey { get; }

    /// <summary>
    /// Keys in the mirror source (e.g. the COMM description <c>Songs-DB_Occasion</c>). At least one;
    /// the first is the primary (authoritative for reads / change-detection).
    /// </summary>
    public IReadOnlyList<TagKey> MirrorKeys { get; }

    /// <summary>The primary mirror key (the first one) — authoritative for reads and change-detection.</summary>
    public TagKey MirrorKey => MirrorKeys[0];

    /// <summary>Maps a field to one or more mirror keys (first is primary).</summary>
    public FieldMirror(FieldId field, TagKey canonicalKey, IReadOnlyList<TagKey> mirrorKeys)
    {
        // field / canonicalKey are value-type ids — never null, so no guard. mirrorKeys is a reference
        // type and this is a public API, so it is guarded (a consumer could still pass null/empty).
        if (mirrorKeys is null || mirrorKeys.Count == 0)
        {
            throw new ArgumentException("A field mirror needs at least one mirror key.", nameof(mirrorKeys));
        }

        Field = field;
        CanonicalKey = canonicalKey;
        MirrorKeys = mirrorKeys;
    }

    /// <summary>Convenience for the common single-mirror case.</summary>
    public FieldMirror(FieldId field, TagKey canonicalKey, TagKey mirrorKey)
        : this(field, canonicalKey, [mirrorKey])
    {
    }
}
