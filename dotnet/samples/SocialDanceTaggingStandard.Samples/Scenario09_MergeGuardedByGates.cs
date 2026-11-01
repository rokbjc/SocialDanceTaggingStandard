using SocialDanceTaggingStandard;
using SocialDanceTaggingStandard.Internal;   // StableHash
using SocialDanceTaggingStandard.Taxonomy;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// Scenario 9 — the same track arrives from two sources (a 320 kbps vinyl rip and a 128 kbps YouTube
/// Music rip). The consumer picks a primary, fills the primary's EMPTY fields from the secondary, and
/// never overwrites a present or gated field. SDTS supplies the gate ledger + StableHash; the
/// pick-primary heuristic and the merge policy are the consumer's (this sample shows a minimal one).
/// </summary>
internal static class MergeGuardedByGates
{
    private sealed record Copy(string Path, int Bitrate, Dictionary<string, string?> Tags);

    public static void Run()
    {
        var vinyl = new Copy("vinyl.mp3", 320, new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["SDTS_DANCES"] = "Lindy Hop",
            ["SDTS_RHYTHMIC_FEEL"] = "Jazz Swing 8ths",
            ["SDTS_CONFIRMATION_GATES"] = $"Genres:1:{StableHash.Of("Swing; Big Band").Substring(0, 4)}",
        });
        var youtube = new Copy("YtMusic.mp3", 128, new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            ["SDTS_DANCES"] = "#LH; Bal",       // richer, but in shorthand
            ["SDTS_THEME"] = "Late Night",      // the primary is missing this
        });

        // Pick primary: prefer the higher-bitrate / curated source (simplified consumer heuristic).
        (Copy primary, Copy secondary) = vinyl.Bitrate >= youtube.Bitrate ? (vinyl, youtube) : (youtube, vinyl);
        Console.WriteLine($"  primary={primary.Path} secondary={secondary.Path}");

        foreach ((string key, string? value) in secondary.Tags)
        {
            if (key == "SDTS_CONFIRMATION_GATES")
            {
                continue;   // gate ledgers are not merged
            }

            if (string.IsNullOrEmpty(primary.Tags.GetValueOrDefault(key)))
            {
                primary.Tags[key] = value;
                Console.WriteLine($"    filled {key} <- '{value}' from secondary");
            }
            else
            {
                Console.WriteLine($"    kept   {key}='{primary.Tags[key]}' (primary already present)");
            }
        }

        // Canonicalize whatever ended up on the primary, the same as a normal normalize pass.
        var vocab = SocialDanceVocabulary.LoadDefault();
        Dimension dances = vocab.Dances(PackIds.Swing, PackIds.Blues);
        string merged = string.Join("; ",
            dances.Normalize((primary.Tags["SDTS_DANCES"] ?? string.Empty).Split(["; "], StringSplitOptions.None))
                  .Select(t => t.Canonical));
        Console.WriteLine($"  merged primary: dances='{merged}' theme='{primary.Tags.GetValueOrDefault("SDTS_THEME")}'");
    }
}
