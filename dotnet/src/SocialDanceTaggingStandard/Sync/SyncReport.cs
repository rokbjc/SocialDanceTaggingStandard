namespace SocialDanceTaggingStandard.Sync;

/// <summary>What happened to one field during a sync.</summary>
public enum FieldSyncOutcome
{
    /// <summary>Both sides already agreed; nothing was written.</summary>
    Unchanged,

    /// <summary>A mirror-side edit was pulled into the canonical field (mirror → canonical).</summary>
    Imported,

    /// <summary>A canonical value was pushed out to the mirror frame (canonical → mirror).</summary>
    Exported,

    /// <summary>Both sides changed since the baseline; resolved or merely reported per policy.</summary>
    Conflict,

    /// <summary>The <see cref="MirrorDirection"/> policy excluded the action that would otherwise apply.</summary>
    Skipped,
}

/// <summary>The result for a single field.</summary>
/// <param name="Field">SDTS field id.</param>
/// <param name="Outcome">What happened.</param>
/// <param name="CanonicalValue">Canonical value after the sync step.</param>
/// <param name="MirrorValue">Mirror value after the sync step.</param>
/// <param name="Resolved">For a conflict, whether it was resolved (vs. left for a human).</param>
public sealed record FieldSyncResult(
    FieldId Field,
    FieldSyncOutcome Outcome,
    string? CanonicalValue,
    string? MirrorValue,
    bool Resolved);

/// <summary>The outcome of a whole sync run.</summary>
/// <param name="Fields">Per-field results, in map order.</param>
public sealed record SyncReport(IReadOnlyList<FieldSyncResult> Fields)
{
    /// <summary>Fields where both sides had diverged.</summary>
    public IEnumerable<FieldSyncResult> Conflicts =>
        Fields.Where(f => f.Outcome == FieldSyncOutcome.Conflict);

    /// <summary>True if any conflict was left for a human (report-only).</summary>
    public bool HasUnresolvedConflicts =>
        Fields.Any(f => f is { Outcome: FieldSyncOutcome.Conflict, Resolved: false });

    /// <summary>Count of fields actually written (imported or exported).</summary>
    public int Changed =>
        Fields.Count(f => f.Outcome is FieldSyncOutcome.Imported or FieldSyncOutcome.Exported);
}
