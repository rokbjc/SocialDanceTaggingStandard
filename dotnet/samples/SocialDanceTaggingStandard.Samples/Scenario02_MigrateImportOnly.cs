using SocialDanceTaggingStandard;
using SocialDanceTaggingStandard.MediaMonkey;
using SocialDanceTaggingStandard.Sync;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// Scenario 2 — a one-time, read-only first pass that lifts a MediaMonkey library's legacy
/// COMM:Songs-DB_* values into portable canonical TXXX:SDTS_* frames. ImportOnly never writes the
/// mirror back and never produces a conflict; it is the right operation for a clean first migration.
/// </summary>
internal static class MigrateImportOnly
{
    public static void Run()
    {
        var canonical = new InMemoryTagSource();   // no SDTS_* yet
        var mirror = new InMemoryTagSource(new Dictionary<string, string?>
        {
            ["Songs-DB_Occasion"] = "Lindy Hop; Balboa",
            ["Songs-DB_Custom4"] = "Jazz Swing 8ths",
            ["Songs-DB_Custom5"] = "Big Band",
            ["Songs-DB_Custom6"] = "AABA",
        });
        var stateStore = new InMemoryTagSource();   // baseline normally lives in TXXX:SDTS_MM5_SYNC

        var options = new MirrorSyncOptions { Direction = MirrorDirection.ImportOnly };
        var bridge = new TagMirror(
            canonical, mirror, stateStore,
            MediaMonkeyFrames.SyncStateKey, MediaMonkeyFrames.Map, options);

        SyncReport report = bridge.Import();   // one-way mirror -> canonical, verbatim
        foreach (FieldSyncResult f in report.Fields)
        {
            Console.WriteLine($"    {f.Field,-16} {f.Outcome}  SDT canonical='{f.CanonicalValue}'");
        }

        bridge.Flush();
        Console.WriteLine($"  baseline written: {stateStore.Get(new TagKey(MediaMonkeyFrames.SyncStateKey))}");
        Console.WriteLine($"  Import() cannot conflict -> unresolvedConflicts={report.HasUnresolvedConflicts}");
    }

    public static void RunWithFile(FileInfo file)
    {
        try
        {
            var options = new MirrorSyncOptions { Direction = MirrorDirection.ImportOnly };
            using MediaMonkeyMirror mirror = MediaMonkeyMirror.Open(file, options);
            SyncReport report = mirror.Import();
            Console.WriteLine($"  [{file.Name}] imported {report.Changed} fields (mirror left untouched)");
            mirror.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  (skipped file variant: {ex.GetType().Name}: {ex.Message})");
        }
    }
}
