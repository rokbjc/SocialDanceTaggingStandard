namespace SocialDanceTaggingStandard.Sync;

/// <summary>
/// How the external mirror (e.g. MediaMonkey's <c>Songs-DB_*</c> frames) is kept in step with
/// the canonical SDTS fields — the "do we sync the MM5 fields, and which way?" switch.
/// </summary>
public enum MirrorDirection
{
    /// <summary>Import mirror edits into canonical <b>and</b> export canonical edits to the mirror (default).</summary>
    Bidirectional,

    /// <summary>The mirror is a read-only legacy source: pull its edits into canonical, never write mirror frames.</summary>
    ImportOnly,

    /// <summary>Canonical is authoritative: overwrite mirror frames from canonical, ignore mirror edits.</summary>
    ExportOnly,

    /// <summary>Do not touch the mirror frames at all (canonical-only; the mirror is abandoned).</summary>
    None,
}

/// <summary>What to do when <b>both</b> sides changed since the last sync — a true conflict.</summary>
public enum ConflictResolution
{
    /// <summary>Leave both values untouched and report the conflict (default — never clobber a real edit).</summary>
    ReportOnly,

    /// <summary>Canonical wins: overwrite the mirror.</summary>
    PreferCanonical,

    /// <summary>Mirror wins: overwrite canonical.</summary>
    PreferMirror,
}

/// <summary>
/// How the sync reads a mirror frame that is <b>absent</b> (the key returns <see langword="null"/>, not
/// an empty value) — the difference between "the DJ cleared this field" and "no signal reached us".
/// </summary>
public enum AbsentMirrorPolicy
{
    /// <summary>
    /// An absent mirror is a cleared value: against a live baseline it reads as mirror-changed and imports
    /// as a deletion of the canonical frame. Correct when the mirror is the single authoritative store, and
    /// the default so existing syncs are unchanged.
    /// </summary>
    Cleared,

    /// <summary>
    /// An absent mirror is unknown, not cleared: it carries no change signal, so it never imports a
    /// deletion. The canonical value stands and is re-materialised to the mirror when <see cref="MirrorDirection"/>
    /// allows. Use this when a mirror frame can vanish for reasons other than a user edit — a foreign-language
    /// <c>COMM</c> the source cannot see, or another tool dropping the frame — so absence never destroys canonical.
    /// </summary>
    Unknown,
}

/// <summary>
/// What <c>Export</c>/<c>Import</c> do with a field's <b>secondary</b> mirror keys — every key after the
/// primary (e.g. a legacy duplicate such as <c>MusicMatch_Situation</c> alongside <c>Songs-DB_Occasion</c>).
/// </summary>
public enum SecondaryMirrorPolicy
{
    /// <summary>
    /// Realign every secondary key to the primary's value so duplicates never drift; a secondary that does
    /// not yet exist is minted. Today's behaviour, and the default.
    /// </summary>
    Realign,

    /// <summary>
    /// Leave secondary keys untouched: only the primary is written. Use this when a secondary is a distinct
    /// legacy frame whose contents must not be flattened onto — nor minted from — the primary's value.
    /// </summary>
    Leave,
}

/// <summary>Options governing a mirror sync. The defaults are the recommended base-library behaviour.</summary>
public sealed record MirrorSyncOptions
{
    /// <summary>Which way edits flow between canonical and mirror. Default <see cref="MirrorDirection.Bidirectional"/>.</summary>
    public MirrorDirection Direction { get; init; } = MirrorDirection.Bidirectional;

    /// <summary>How both-sides-changed conflicts are handled. Default <see cref="ConflictResolution.ReportOnly"/>.</summary>
    public ConflictResolution OnConflict { get; init; } = ConflictResolution.ReportOnly;

    /// <summary>
    /// Behaviour when a canonical (primary) field is changed through
    /// <see cref="ITagMirror.WriteCanonical"/>: when <see langword="true"/> the mirror frame and the
    /// baseline are updated immediately (subject to <see cref="Direction"/>); when <see langword="false"/>
    /// the mirror is left untouched until the next <see cref="ITagMirror.Sync"/>. This is the
    /// "sync the MM5 fields on a primary-field change?" knob. Default <see langword="true"/>.
    /// </summary>
    public bool WriteThrough { get; init; } = true;

    /// <summary>
    /// How an absent mirror frame is read. Default <see cref="AbsentMirrorPolicy.Cleared"/> (absence deletes
    /// canonical, today's behaviour); <see cref="AbsentMirrorPolicy.Unknown"/> treats absence as no-signal so
    /// a vanished mirror never destroys the canonical value.
    /// </summary>
    public AbsentMirrorPolicy AbsentMirror { get; init; } = AbsentMirrorPolicy.Cleared;

    /// <summary>
    /// What happens to a field's secondary mirror keys on write. Default <see cref="SecondaryMirrorPolicy.Realign"/>
    /// (secondaries follow the primary, today's behaviour); <see cref="SecondaryMirrorPolicy.Leave"/> writes only
    /// the primary so a distinct legacy secondary frame is neither flattened nor minted.
    /// </summary>
    public SecondaryMirrorPolicy SecondaryMirror { get; init; } = SecondaryMirrorPolicy.Realign;

    /// <summary>The recommended defaults: bidirectional, report-only conflicts, write-through,
    /// absent-mirror-cleared, secondary-keys-realigned.</summary>
    public static MirrorSyncOptions Default { get; } = new();
}
