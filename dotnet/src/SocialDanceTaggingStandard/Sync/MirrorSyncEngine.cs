using SocialDanceTaggingStandard.Internal;

namespace SocialDanceTaggingStandard.Sync;

/// <summary>
/// The value-agnostic 3-way reconcile between a canonical SDTS source and an external mirror.
/// A per-field baseline (last-synced hash, from <see cref="SyncState"/>) tells which side changed:
/// <list type="bullet">
/// <item>only the mirror changed → import (mirror → canonical);</item>
/// <item>only canonical changed → export (canonical → mirror);</item>
/// <item>both changed → a conflict, handled per <see cref="ConflictResolution"/>;</item>
/// <item>no baseline yet → first sync: copy the present side, or conflict if both differ.</item>
/// </list>
/// The engine never inspects field <i>values</i>; comparison is by content hash and value mapping
/// is delegated to an <see cref="IValueTranslator"/> (identity until a taxonomy exists).
/// </summary>
public sealed class MirrorSyncEngine
{
    private readonly IValueTranslator _translator;

    /// <summary>Creates an engine. With no translator, mirror and canonical values are identical (pass-through).</summary>
    public MirrorSyncEngine(IValueTranslator? translator = null) =>
        _translator = translator ?? IdentityValueTranslator.Instance;

    /// <summary>Reconciles every mapped field, mutating <paramref name="state"/> and the two sources in place.</summary>
    public SyncReport Reconcile(
        ISdtsSource canonical,
        ISdtsSource mirror,
        SyncState state,
        IEnumerable<FieldMirror> map,
        MirrorSyncOptions options)
    {
        var results = new List<FieldSyncResult>();
        foreach (FieldMirror m in map)
        {
            results.Add(ReconcileField(canonical, mirror, state, m, options));
        }

        return new SyncReport(results);
    }

    private FieldSyncResult ReconcileField(
        ISdtsSource canonical, ISdtsSource mirror, SyncState state, FieldMirror m, MirrorSyncOptions options)
    {
        string? canonicalValue = canonical.Get(m.CanonicalKey);
        string? rawMirror = mirror.Get(m.MirrorKey);
        string? mirrorAsCanonical = _translator.MirrorToCanonical(m.Field, rawMirror);

        string canonicalHash = StableHash.Of(canonicalValue);
        string mirrorHash = StableHash.Of(mirrorAsCanonical);
        string? baseline = state.BaselineHash(m.Field);

        bool canonicalChanged = baseline is null ? canonicalValue is not null : canonicalHash != baseline;
        bool mirrorChanged = baseline is null ? mirrorAsCanonical is not null : mirrorHash != baseline;

        // Already consistent — record the agreed hash and move on.
        if (canonicalHash == mirrorHash)
        {
            state.Set(m.Field, canonicalHash);
            return new FieldSyncResult(m.Field, FieldSyncOutcome.Unchanged, canonicalValue, rawMirror, true);
        }

        // An ABSENT mirror (the frame does not exist) carries no signal under AbsentMirrorPolicy.Unknown:
        // its absence must never import as a deletion of a present canonical value. Canonical stands; it is
        // re-exported so a mirror that was dropped elsewhere heals (a no-op when direction forbids export).
        if (rawMirror is null
            && options.AbsentMirror == AbsentMirrorPolicy.Unknown
            && canonicalValue is not null)
        {
            return Export(mirror, state, m, options, canonicalValue, conflict: false);
        }

        if (mirrorChanged && !canonicalChanged)
        {
            return Import(canonical, mirror, state, m, options, rawMirror, mirrorAsCanonical, conflict: false);
        }

        if (canonicalChanged && !mirrorChanged)
        {
            return Export(mirror, state, m, options, canonicalValue, conflict: false);
        }

        // Both diverged (or first sight with two different present values) → conflict.
        return ResolveConflict(canonical, mirror, state, m, options, canonicalValue, rawMirror, mirrorAsCanonical);
    }

    private FieldSyncResult Import(
        ISdtsSource canonical,
        ISdtsSource mirror,
        SyncState state,
        FieldMirror m,
        MirrorSyncOptions options,
        string? rawMirror,
        string? mirrorAsCanonical,
        bool conflict)
    {
        if (options.Direction is MirrorDirection.None or MirrorDirection.ExportOnly)
        {
            return new FieldSyncResult(m.Field, FieldSyncOutcome.Skipped, canonical.Get(m.CanonicalKey), rawMirror, false);
        }

        canonical.Set(m.CanonicalKey, mirrorAsCanonical);

        // Realign secondary mirror keys to the primary so duplicates never drift — unless the caller asked
        // to leave them alone (a distinct legacy secondary frame must not be flattened onto the primary).
        if (options.SecondaryMirror == SecondaryMirrorPolicy.Realign)
        {
            string? toMirror = _translator.CanonicalToMirror(m.Field, mirrorAsCanonical);
            for (int i = 1; i < m.MirrorKeys.Count; i++)
            {
                mirror.Set(m.MirrorKeys[i], toMirror);
            }
        }

        state.Set(m.Field, StableHash.Of(mirrorAsCanonical));
        return new FieldSyncResult(
            m.Field, conflict ? FieldSyncOutcome.Conflict : FieldSyncOutcome.Imported, mirrorAsCanonical, rawMirror, true);
    }

    private FieldSyncResult Export(
        ISdtsSource mirror,
        SyncState state,
        FieldMirror m,
        MirrorSyncOptions options,
        string? canonicalValue,
        bool conflict)
    {
        if (options.Direction is MirrorDirection.None or MirrorDirection.ImportOnly)
        {
            return new FieldSyncResult(m.Field, FieldSyncOutcome.Skipped, canonicalValue, mirror.Get(m.MirrorKey), false);
        }

        string? toMirror = _translator.CanonicalToMirror(m.Field, canonicalValue);
        mirror.Set(m.MirrorKey, toMirror);

        // Secondary keys follow the primary by default; SecondaryMirrorPolicy.Leave writes only the primary,
        // so a legacy secondary frame is neither flattened onto nor minted from the primary's value.
        if (options.SecondaryMirror == SecondaryMirrorPolicy.Realign)
        {
            for (int i = 1; i < m.MirrorKeys.Count; i++)
            {
                mirror.Set(m.MirrorKeys[i], toMirror);
            }
        }
        state.Set(m.Field, StableHash.Of(canonicalValue));
        return new FieldSyncResult(
            m.Field, conflict ? FieldSyncOutcome.Conflict : FieldSyncOutcome.Exported, canonicalValue, toMirror, true);
    }

    private FieldSyncResult ResolveConflict(
        ISdtsSource canonical, ISdtsSource mirror, SyncState state, FieldMirror m, MirrorSyncOptions options,
        string? canonicalValue, string? rawMirror, string? mirrorAsCanonical)
    {
        return options.OnConflict switch
        {
            ConflictResolution.PreferCanonical => Export(mirror, state, m, options, canonicalValue, conflict: true),
            ConflictResolution.PreferMirror => Import(canonical, mirror, state, m, options, rawMirror, mirrorAsCanonical, conflict: true),
            _ => new FieldSyncResult(m.Field, FieldSyncOutcome.Conflict, canonicalValue, rawMirror, false),// Report only: touch nothing and leave the baseline as-is, so it re-surfaces until resolved.
        };
    }
}
