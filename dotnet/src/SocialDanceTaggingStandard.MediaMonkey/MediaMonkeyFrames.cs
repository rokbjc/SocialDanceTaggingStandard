using SocialDanceTaggingStandard.Id3;
using SocialDanceTaggingStandard.Sync;

namespace SocialDanceTaggingStandard.MediaMonkey;

/// <summary>
/// The structural correspondence between canonical SDTS fields (the <c>TXXX:SDTS_*</c> frames) and
/// the legacy MediaMonkey <c>COMM:"Songs-DB_*"</c> frames that hold them on existing files.
/// <para>
/// Frame names are frozen identities; this is a <b>key map only</b>. There is deliberately
/// <b>no value translation</b> — mapping, say, <c>#LH</c> ⇄
/// <c>Lindy Hop</c> is the taxonomy layer's job, not this key map's, so the bridge moves raw strings verbatim
/// (<see cref="IdentityValueTranslator"/>).
/// </para>
/// </summary>
public static class MediaMonkeyFrames
{
    /// <summary>The COMM language MediaMonkey writes its <c>Songs-DB_*</c> frames under (the ISO-639-2
    /// "unspecified" code). Sourced from the single <see cref="Id3Language.Unspecified"/> definition,
    /// never re-spelled here.</summary>
    public static readonly Id3Language Language = Id3Language.Unspecified;

    /// <summary>Legacy MusicMatch (MM4-era) duplicate of the occasion frame, kept aligned for back-compat.
    /// A reference-implementation legacy frame with no <c>data/fields.yaml</c> field of its own, so it is
    /// hand-defined here (guideline 2) — the same exception class as <see cref="SyncStateKey"/>.</summary>
    public const string MusicMatchSituation = "MusicMatch_Situation";

    /// <summary>TXXX description holding this bridge's per-field sync baseline. Reference-implementation
    /// private state with no <c>data/fields.yaml</c> source, so it is hand-defined here (guideline 2).</summary>
    public const string SyncStateKey = "SDTS_MM5_SYNC";

    /// <summary>
    /// Canonical SDTS_* field key ⇄ MediaMonkey COMM <c>Songs-DB_*</c> description, for the fields flagged
    /// <c>mm5_mirror: true</c> in <c>data/fields.yaml</c> (the "reused-slot ↔ column" set in
    /// <c>docs/sync/tool-comparison.md</c>). The 1:1 rows are generated into <see cref="MediaMonkeyMirrorMap"/>
    /// from the typed FieldId / SdtsFrames / MmColumns constants; this facade additionally grafts the legacy
    /// <see cref="MusicMatchSituation"/> on as a SECOND mirror of <c>dances</c> (a back-compat frame with no
    /// generated field of its own), so exports keep the MM4-era duplicate aligned.
    /// </summary>
    public static IReadOnlyList<FieldMirror> Map { get; } = BuildMap();

    private static IReadOnlyList<FieldMirror> BuildMap() =>
        [.. MediaMonkeyMirrorMap.Map.Select(m => m.Field == FieldIds.Dance
            ? new FieldMirror(m.Field, m.CanonicalKey, [m.MirrorKey, new TagKey(MusicMatchSituation)])
            : m)];
}
