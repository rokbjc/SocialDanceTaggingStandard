namespace SocialDanceTaggingStandard.Sync;

/// <summary>
/// A named external "mirror" of the canonical SDTS fields (for example, MediaMonkey's legacy
/// <c>COMM:Songs-DB_*</c> frames). It bundles the canonical and mirror sources, the structural
/// field map, and the sync baseline, exposing the three operations a binding extension provides:
/// <b>import</b>, <b>export</b>, and the smart 3-way <b>sync</b>.
/// <para>Mutations are in memory; call <see cref="Flush"/> to persist them.</para>
/// </summary>
public interface ITagMirror
{
    /// <summary>One-way pull: copy every mapped mirror value into canonical (mirror wins). Updates the baseline.</summary>
    SyncReport Import();

    /// <summary>One-way push: copy every canonical value out to the mirror frames (canonical wins). Updates the baseline.</summary>
    SyncReport Export();

    /// <summary>3-way reconcile: import mirror edits, export canonical edits, and handle real conflicts per policy.</summary>
    SyncReport Sync();

    /// <summary>
    /// Writes a canonical (primary) field, honouring the mirror-write policy: under
    /// <see cref="MirrorSyncOptions.WriteThrough"/> with an exporting direction the mirror frame and
    /// baseline update immediately; otherwise the mirror is left for the next <see cref="Sync"/>.
    /// Only fields present in the mirror map are accepted.
    /// </summary>
    void WriteCanonical(string field, string? value);

    /// <summary>Reads a canonical (primary) field's current value by field id. Only mapped fields are accepted.</summary>
    string? GetCanonical(string field);

    /// <summary>Persists the baseline and any pending writes (e.g. saves the underlying file).</summary>
    void Flush();
}
