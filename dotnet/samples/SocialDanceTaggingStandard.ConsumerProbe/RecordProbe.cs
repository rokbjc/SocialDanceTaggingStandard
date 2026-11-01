using SocialDanceTaggingStandard;
using SocialDanceTaggingStandard.Id3;

namespace SocialDanceTaggingStandard.ConsumerProbe;

/// <summary>Compile-proof: a consumer touches every record category through SdtsFile and names no
/// media-library type from the underlying tagging engine. If this compiles against only
/// SocialDanceTaggingStandard.Id3, the gateway is self-sufficient.</summary>
public static class RecordProbe
{
    public static void RoundTrip(FileInfo source)
    {
        using var file = SdtsFile.OpenId3(source);

        file.Dances = ["Lindy Hop", "Balboa"];          // SDTS TXXX field (typed)
        file.PlaybackStart = System.TimeSpan.FromSeconds(8);
        file.EndingType = EndingType.Definitive;
        file.Gates = file.Gates.WithGate(FieldIds.Bpm, 1);

        file.Container.Genres = ["Swing"];              // standard TCON via the container
        file.Container.Bpm = 250;                       // TBPM
        file.Container.Rating = 196;                    // POPM byte
        var duration = file.Container.Duration;         // audio prop (TimeSpan)
        var _ = file.Container.AudioBitrate;

        file.Set(new TagKey("SG_ENERGY"), "7");          // arbitrary TXXX by key
        file.Save();
    }
}
