using SocialDanceTaggingStandard.Taxonomy;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// Scenario 12 — per-deployment vocabulary customization with the shipped overlay API. Start from the
/// immutable baseline and layer edits with the fluent <see cref="VocabularyCustomizer"/>: RemoveAlias to
/// drop the legacy '#' codes, AddAlias for house shorthand, AddTerm for a core dimension. Build() returns
/// a NEW vocabulary; the baseline is never mutated. (VALUE customization lives here; field→frame MAPPING
/// customization stays in the Sync layer — additionalFields / MirrorMap.Compose, scenario 10.)
/// </summary>
internal static class CustomizeVocabulary
{
    public static void Run()
    {
        var baseline = SocialDanceVocabulary.LoadDefault();
        Console.WriteLine($"  baseline: '#LH' -> '{baseline.Dances(PackIds.Swing, PackIds.Blues).Resolve("#LH")?.Canonical}'");

        var custom = baseline.Customize()
            .RemoveAlias(SocialDanceVocabulary.DancesDimension, "#LH")                        // opt out of the legacy '#' convention
            .AddAlias(DimensionIds.Genre, "Swing", "My Swing")             // house shorthand
            .AddTerm(DimensionIds.Genre, new VocabularyTerm("balkan-brass", "Balkan Brass",
                     ["Brass BG"], false, new Dictionary<string, object?>()))
            .Build();                                           // -> NEW vocabulary; baseline untouched

        var dances = custom.Dances(PackIds.Swing, PackIds.Blues);
        Console.WriteLine($"  custom:   '#LH' -> '{dances.Resolve("#LH")?.Canonical ?? "(dropped)"}', bare 'LH' -> '{dances.Resolve("LH")?.Canonical}'");
        Console.WriteLine($"  custom:   'My Swing' -> '{custom.Dimension(DimensionIds.Genre)?.Resolve("My Swing")?.Canonical}'");
        Console.WriteLine($"  custom:   added term 'Balkan Brass' present: {custom.Dimension(DimensionIds.Genre)?.Contains("Balkan Brass")}");
        Console.WriteLine($"  baseline still intact: '#LH' -> '{baseline.Dances(PackIds.Swing, PackIds.Blues).Resolve("#LH")?.Canonical}'");
        Console.WriteLine("  (YAML equivalent: LoadDefault(new FileInfo(\"my.sdvo.yaml\")) — see data/vocabulary/vocabulary.local.example.sdvo.yaml)");
    }
}
