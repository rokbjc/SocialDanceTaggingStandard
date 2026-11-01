using SocialDanceTaggingStandard;
using SocialDanceTaggingStandard.Internal;   // StableHash
using SocialDanceTaggingStandard.MediaMonkey;
using SocialDanceTaggingStandard.Sync;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// Scenario 4 — the same track's dances get edited in foobar2000 (canonical TXXX:SDTS_DANCES) AND in
/// MediaMonkey (mirror COMM:Songs-DB_Occasion) since the last sync. Both sides moved off the baseline,
/// so it is a true 3-way conflict: ReportOnly surfaces it without clobbering; PreferCanonical lands it.
/// A one-sided rhythmic_feel edit imports cleanly in the same run.
/// </summary>
internal static class CrossAppConflict
{
    public static void Run()
    {
        var canonical = new InMemoryTagSource(new Dictionary<string, string?>
        {
            ["SDTS_DANCES"] = "Lindy Hop; Balboa",     // edited in foobar2000
            ["SDTS_RHYTHMIC_FEEL"] = "Jazz Swing 8ths",  // unchanged since baseline
        });
        var mirror = new InMemoryTagSource(new Dictionary<string, string?>
        {
            ["Songs-DB_Occasion"] = "Lindy Hop; Collegiate Shag",   // edited in MediaMonkey
            ["Songs-DB_Custom4"] = "Two-Beat",                      // one-sided feel edit
        });

        // Baseline: both sides last agreed on these.
        var state = new SyncState();
        state.Set(new FieldId("dances"), StableHash.Of("Lindy Hop"));
        state.Set(new FieldId("rhythmic_feel"), StableHash.Of("Jazz Swing 8ths"));

        var engine = new MirrorSyncEngine();

        // 1) Dry run with the defaults (Bidirectional, ReportOnly): surface, do not clobber.
        SyncReport report = engine.Reconcile(canonical, mirror, state, MediaMonkeyFrames.Map, MirrorSyncOptions.Default);
        foreach (FieldSyncResult f in report.Fields.Where(f => f.Outcome != FieldSyncOutcome.Unchanged))
        {
            Console.WriteLine($"    {f.Field,-16} {f.Outcome} resolved={f.Resolved}  c='{f.CanonicalValue}' m='{f.MirrorValue}'");
        }
        Console.WriteLine($"  unresolvedConflicts={report.HasUnresolvedConflicts}");

        // 2) Mara judges the foobar edit authoritative: canonical wins.
        SyncReport resolved = engine.Reconcile(
            canonical, mirror, state, MediaMonkeyFrames.Map,
            MirrorSyncOptions.Default with { OnConflict = ConflictResolution.PreferCanonical });
        Console.WriteLine($"  after PreferCanonical: Occasion='{mirror.Get(new TagKey("Songs-DB_Occasion"))}' unresolved={resolved.HasUnresolvedConflicts}");
    }
}
