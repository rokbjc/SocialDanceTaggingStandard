using SocialDanceTaggingStandard;
using SocialDanceTaggingStandard.Id3;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// Scenario 7 — portable cue points. A social DJ wants tracks to drop straight into the groove and to
/// cut a long applause tail. JRiver ([Playback Range]) and iTunes (Start/Stop Time) do this but keep it
/// in their own database, so it is lost the moment the file moves machines. The proposed SDTS cue
/// fields write it to the file as plain TXXX scalars, so it travels with the track.
/// (These fields are status: proposed in data/fields.yaml — the descriptions below are their planned home.)
/// </summary>
internal static class CuePoints
{
    public static void Run()
    {
        // 8 s spoken intro, hard cut at 3:15, clean instant entry, definitive ending — typed, no raw keys.
        var file = new SdtsRecord(new InMemoryTagSource());
        file.PlaybackStart = TimeSpan.FromSeconds(8);
        file.PlaybackStop = TimeSpan.FromMilliseconds(195000);
        file.IntroDuration = TimeSpan.FromSeconds(8);
        file.OutroDuration = TimeSpan.Zero;
        file.FadeInDuration = TimeSpan.Zero;
        file.EndingType = EndingType.Definitive;

        Console.WriteLine($"  player: cue-in {file.PlaybackStart?.TotalMilliseconds} ms .. cue-out {file.PlaybackStop?.TotalMilliseconds} ms, ending={file.EndingType}, intro={file.IntroDuration?.TotalMilliseconds} ms");
        Console.WriteLine("  (portable in the file, unlike JRiver/iTunes which keep cue points DB-only)");
    }

    public static void RunWithFile(FileInfo file)
    {
        try
        {
            SdtsFile.Edit(file, f =>
            {
                f.PlaybackStart = TimeSpan.FromSeconds(8);
                f.EndingType = EndingType.Definitive;
            });

            using var sdts = SdtsFile.OpenId3(file);
            Console.WriteLine($"  [{file.Name}] wrote PlaybackStart={sdts.PlaybackStart?.TotalMilliseconds} ms, EndingType={sdts.EndingType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  (skipped file variant: {ex.GetType().Name}: {ex.Message})");
        }
    }
}
