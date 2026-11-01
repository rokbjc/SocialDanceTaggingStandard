using SocialDanceTaggingStandard;
using SocialDanceTaggingStandard.MediaMonkey;
using SocialDanceTaggingStandard.Sync;
using SocialDanceTaggingStandard.Taxonomy;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// Scenario 10 — how a consumer app consumes SDTS. The library ships dependency-light; the
/// consumer composes it: wire the shipped <see cref="TaxonomyValueTranslator"/> (so #LH ⇄ Lindy Hop is
/// resolved inside sync), keep its own private SB-* caches on the same file (which the engine never touches
/// because they are not mapped), and add extra private mirrored fields via additionalFields.
/// Confirmation-gate enforcement is the consumer's policy.
/// </summary>
internal static class ConsumerIntegration
{
    public static void Run()
    {
        var vocab = SocialDanceVocabulary.LoadDefault();
        var translator = new TaxonomyValueTranslator(vocab, "; ", PackIds.Swing, PackIds.Blues);   // shipped seam

        // The consumer opens the file once; the SDTS canonical fields AND its private SB-* caches
        // ride the same tag source.
        var canonical = new InMemoryTagSource();
        var mirror = new InMemoryTagSource(new Dictionary<string, string?>
        {
            ["Songs-DB_Occasion"] = "#LH; Bal",   // legacy MM shorthand
            ["Songs-DB_Custom4"] = "Shuffle",     // rhythmic_feel
        });
        canonical.Set(new TagKey("SB-Beats"), "[120,540,960]");   // private analysis cache — NOT an SDTS field
        var state = new SyncState();

        // Same engine, now value-aware via the taxonomy translator instead of identity.
        var engine = new MirrorSyncEngine(translator);
        engine.Reconcile(canonical, mirror, state, MediaMonkeyFrames.Map, MirrorSyncOptions.Default);

        Console.WriteLine($"  dances canonicalized inside sync: '{canonical.Get(new TagKey("SDTS_DANCES"))}'");  // Lindy Hop; Balboa
        Console.WriteLine($"  rhythmic_feel: '{canonical.Get(new TagKey("SDTS_RHYTHMIC_FEEL"))}'");
        Console.WriteLine($"  private SB-Beats untouched (not a mapped field): '{canonical.Get(new TagKey("SB-Beats"))}'");

        // Gate policy is the consumer's: read the ledger, refuse to overwrite a gated field.
        canonical.Set(new TagKey("SDTS_CONFIRMATION_GATES"), "BPM:1; Genres:1");
        bool bpmGated = (canonical.Get(new TagKey("SDTS_CONFIRMATION_GATES")) ?? "").Contains("BPM:");
        Console.WriteLine($"  BPM gated -> re-analysis overwrite refused: {bpmGated}");
    }

    public static void RunWithFile(FileInfo file)
    {
        try
        {
            var vocab = SocialDanceVocabulary.LoadDefault();
            // Extend the standard 7-field MM map with a private mirrored field (its own COMM slot).
            var extra = new[] { new FieldMirror(new FieldId("sb_energy"), new TagKey("SB-Energy"), new TagKey("Songs-DB_Custom2")) };
            using var mm = MediaMonkeyMirror.Open(
                file, MirrorSyncOptions.Default,
                translator: new TaxonomyValueTranslator(vocab, "; ", PackIds.Swing, PackIds.Blues),
                additionalFields: extra);   // MirrorMap.Compose merges + rejects collisions
            SyncReport report = mm.Sync();
            Console.WriteLine($"  [{file.Name}] synced {report.Changed} fields (standard map + private sb_energy)");
            mm.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  (skipped file variant: {ex.GetType().Name}: {ex.Message})");
        }
    }
}
