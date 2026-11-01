using SocialDanceTaggingStandard.Taxonomy;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// Scenario 3 — a set planner reads SDTS-tagged tracks to build a per-dance, per-tempo pool for a
/// beginner class slot (instrumental Slow Drag, 60-100 BPM) and an opener->peak->cooldown arc.
/// The danceability axes (dances + rhythmic_feel + the proposed energy field) are what genre and BPM
/// alone cannot express. (Tracks are modelled in-memory here; on a real file you read the TXXX:SDTS_*
/// frames via Id3UserTextSource and bpm off the standard TBPM frame.)
/// </summary>
internal static class SetBuilder
{
    private sealed record Track(string Path, string Dances, string Feel, string Vocality, string Mood, int Bpm, string? Energy);

    public static void Run()
    {
        var vocab = SocialDanceVocabulary.LoadDefault();
        Dimension danceDim = vocab.Dances(PackIds.Swing, PackIds.Blues);
        Dimension feelDim = vocab.Dimension(DimensionIds.RhythmicFeel)!;
        Dimension vocalDim = vocab.Dimension(DimensionIds.Vocality)!;
        Dimension moodDim = vocab.Dimension(DimensionIds.Mood)!;
        VocabularyTerm? slowDrag = feelDim.Resolve("Slow Drag");

        var library = new[]
        {
            new Track("a.mp3", "#BLS; #SD", "Slow Drag", "Instrumental", "Bluesy", 72, null),
            new Track("b.mp3", "Blues", "Shuffle", "Female", "Hot", 96, "7"),
            new Track("c.mp3", "Lindy Hop; Balboa", "Jazz Swing 8ths", "Group", "Joyful", 184, "8"),
            new Track("d.mp3", "Slow Drag", "Slow Drag", "Female", "Melancholic", 64, "3"),
        };

        var classPool = new List<string>();
        var social = new List<(string Path, int Energy)>();

        foreach (Track t in library)
        {
            var dances = danceDim.Normalize(t.Dances.Split(["; "], StringSplitOptions.None));
            if (!dances.Any(d => d.Id is "blues-dancing" or "slow-drag"))
            {
                continue;   // not a blues-floor track for tonight
            }

            VocabularyTerm? vocality = vocalDim.Resolve(t.Vocality);
            VocabularyTerm? feel = feelDim.Resolve(t.Feel);

            if (vocality?.Id == "instrumental" && feel?.Id == slowDrag?.Id && t.Bpm is >= 60 and <= 100)
            {
                classPool.Add($"{t.Path}@{t.Bpm}");
            }

            int energy = int.TryParse(t.Energy, System.Globalization.CultureInfo.InvariantCulture, out int e) ? e : MoodEnergy(moodDim.Resolve(t.Mood));
            social.Add((t.Path, energy));
        }

        Console.WriteLine($"  class pool (instrumental Slow Drag, 60-100 BPM): {string.Join(", ", classPool)}");
        Console.WriteLine($"  social arc by energy: {string.Join(" -> ", social.OrderBy(x => x.Energy).Select(x => $"{x.Path}({x.Energy})"))}");
    }

    // energy is a PROPOSED field; when absent, fall back to the mood term's 1–10 energy axis.
    private static int MoodEnergy(VocabularyTerm? mood)
    {
        if (mood is null)
        {
            return 5;
        }

        return Moods.ById.TryGetValue(mood.Id, out var m) ? m.Energy?.Value ?? 5 : 5;
    }
}
