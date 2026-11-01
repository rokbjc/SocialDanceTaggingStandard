using SocialDanceTaggingStandard;
using SocialDanceTaggingStandard.MediaMonkey;
using SocialDanceTaggingStandard.Sync;
using SocialDanceTaggingStandard.Taxonomy;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// Scenario 1 — a consumer's batch "Normalize to SDTS" pass over a swing/blues crate.
/// Pull the legacy MediaMonkey COMM:Songs-DB_* columns into the portable canonical TXXX:SDTS_*
/// frames, keep MM in step, then canonicalize the imported dances through the taxonomy (the engine
/// itself only moves raw strings — identity translation is the only translator that exists today).
/// </summary>
internal static class NormalizeToSdts
{
    public static void Run()
    {
        // First pass: only the legacy MM columns are populated; the SDTS_* side is empty.
        var canonical = new InMemoryTagSource();
        var mirror = new InMemoryTagSource(new Dictionary<string, string?>
        {
            ["Songs-DB_Occasion"] = "#LH; Bal",        // dances, in MediaMonkey shorthand
            ["Songs-DB_Custom4"] = "Jazz Swing 8ths",  // rhythmic_feel
            ["Songs-DB_Custom5"] = "Big Band",         // instrumentation
            ["Songs-DB_Custom6"] = "AABA",             // song_form
        });
        var state = new SyncState();

        var options = MirrorSyncOptions.Default with
        {
            Direction = MirrorDirection.Bidirectional,
            OnConflict = ConflictResolution.ReportOnly,   // never guess a real two-sided conflict
            WriteThrough = true,
        };

        // MediaMonkeyFrames.Map is the standard 7-field key map (dances, vocality, rhythmic_feel,
        // instrumentation, song_form, instrument_led, theme). bpm/genre/mood/rating are NOT here —
        // they live in standard frames (TBPM/TCON/TMOO/POPM), outside this mirror.
        var engine = new MirrorSyncEngine();
        SyncReport report = engine.Reconcile(canonical, mirror, state, MediaMonkeyFrames.Map, options);

        Console.WriteLine($"  changed={report.Changed} unresolvedConflicts={report.HasUnresolvedConflicts}");
        foreach (FieldSyncResult f in report.Fields.Where(f => f.Outcome != FieldSyncOutcome.Unchanged))
        {
            Console.WriteLine($"    {f.Field,-16} {f.Outcome}  canonical='{f.CanonicalValue}'");
        }

        // Canonicalize the imported dances ourselves (#LH -> Lindy Hop): the sync seam is value-agnostic.
        var vocab = SocialDanceVocabulary.LoadDefault();
        Dimension dances = vocab.Dances(PackIds.Swing, PackIds.Blues);
        string? raw = canonical.Get(new TagKey("SDTS_DANCES"));                       // "#LH; Bal" (verbatim import)
        if (raw is not null)
        {
            var normalized = dances.Normalize(raw.Split(["; "], StringSplitOptions.None));
            string cleaned = string.Join("; ", normalized.Select(t => t.Canonical));
            Console.WriteLine($"  canonicalize dances: '{raw}' -> '{cleaned}'");   // -> "Lindy Hop; Balboa"
            canonical.Set(new TagKey("SDTS_DANCES"), cleaned);
        }
    }

    /// <summary>The real path over an actual MP3 uses the MediaMonkey bridge end to end.</summary>
    public static void RunWithFile(FileInfo file)
    {
        try
        {
            var options = MirrorSyncOptions.Default with { OnConflict = ConflictResolution.ReportOnly };
            using MediaMonkeyMirror mirror = MediaMonkeyMirror.Open(file, options);
            SyncReport report = mirror.Sync();
            Console.WriteLine($"  [{file.Name}] changed={report.Changed} conflicts={report.HasUnresolvedConflicts}");
            mirror.Flush();   // persists the SDTS_MM5_SYNC baseline + saves the file
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  (skipped file variant: {ex.GetType().Name}: {ex.Message})");
        }
    }
}
