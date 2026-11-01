using System.Text.RegularExpressions;
using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>
/// Drift-gate for the generated per-dimension term catalogs. Each catalog (<see cref="Genres"/>,
/// <see cref="Moods"/>, … <see cref="Dances"/>) is a compile-time typed view of the same
/// <c>data/vocabulary/</c> the runtime <see cref="SocialDanceVocabulary"/> loads — so the two can only
/// diverge through a generator bug. Every term must match value-for-value (code, canonical, aliases,
/// description), which keeps the typed catalog honest without it becoming a hand-maintained mirror.
/// </summary>
public sealed class GeneratedVocabularyCatalogTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();

    private static IReadOnlyList<VocabularyTerm> Dim(string name) =>
        Vocab.Dimension(new DimensionId(name))?.Terms ?? throw new Xunit.Sdk.XunitException($"runtime dimension '{name}' missing");

    private static IReadOnlyList<VocabularyTerm> PackDances(PackId name) => Vocab.Packs[name].Dances.Terms;

    // Descriptions are folded YAML scalars; compare on collapsed whitespace so the gate flags real content
    // drift, not a stray newline difference between the Python generator and the C# loader.
    private static string Norm(string? s) => Regex.Replace(s ?? "", @"\s+", " ").Trim();

    private static void AssertCatalog<T>(
        IReadOnlyList<T> generated,
        IReadOnlyList<VocabularyTerm> runtime,
        Func<T, (string Code, string Canonical, IReadOnlyList<string> Aliases, string Description)> project)
    {
        Assert.Equal(runtime.Count, generated.Count);
        foreach (var item in generated)
        {
            var (code, canonical, aliases, description) = project(item);
            var term = runtime.Single(t => t.Canonical == canonical);
            Assert.Equal(term.Display?.Code ?? "", code);
            Assert.Equal(term.Aliases, aliases);
            Assert.Equal(Norm(Convert.ToString(term.Attributes.GetValueOrDefault("description"))), Norm(description));
        }
    }

    // The generated CatalogAttributeDrift.Check projects each typed catalog attribute back to its YAML
    // token and yields any mismatch against the runtime Attributes bag — so the typed surface (enums,
    // ints, lists, BpmRange, TermAudio) can only drift from the YAML through a generator bug.
    private static void AssertAttrs<T>(
        IReadOnlyList<T> generated,
        IReadOnlyList<VocabularyTerm> runtime,
        Func<T, string> canonical,
        Func<T, VocabularyTerm, IEnumerable<string>> check)
    {
        Assert.Equal(runtime.Count, generated.Count);  // a dropped term must fail here, not only in the sibling catalog test
        foreach (var item in generated)
        {
            var term = runtime.Single(t => t.Canonical == canonical(item));
            var mismatches = check(item, term).ToList();
            Assert.True(mismatches.Count == 0, string.Join("\n", mismatches));
        }
    }

    [Fact] public void GenreAttrs_MatchRuntime() => AssertAttrs(Genres.All, Dim("genre"), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void MoodAttrs_MatchRuntime() => AssertAttrs(Moods.All, Dim("mood"), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void RhythmicFeelAttrs_MatchRuntime() => AssertAttrs(RhythmicFeels.All, Dim("rhythmic_feel"), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void VocalityAttrs_MatchRuntime() => AssertAttrs(Vocalities.All, Dim("vocality"), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void InstrumentationAttrs_MatchRuntime() => AssertAttrs(Instrumentations.All, Dim("instrumentation"), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void InstrumentLedAttrs_MatchRuntime() => AssertAttrs(Instruments.All, Dim("instrument"), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void RecordingContextAttrs_MatchRuntime() => AssertAttrs(RecordingContexts.All, Dim("recording_context"), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void RecordingQualityEraAttrs_MatchRuntime() => AssertAttrs(RecordingQualityEras.All, Dim("recording_quality_era"), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void ThemeAttrs_MatchRuntime() => AssertAttrs(Themes.All, Dim("theme"), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void SongFormAttrs_MatchRuntime() => AssertAttrs(SongForms.All, Dim("song_form"), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void SwingDanceAttrs_MatchRuntime() => AssertAttrs(Dances.Swing.All, PackDances(PackIds.Swing), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void BluesDanceAttrs_MatchRuntime() => AssertAttrs(Dances.Blues.All, PackDances(PackIds.Blues), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void TangoDanceAttrs_MatchRuntime() => AssertAttrs(Dances.Tango.All, PackDances(PackIds.Tango), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void BallroomDanceAttrs_MatchRuntime() => AssertAttrs(Dances.Ballroom.All, PackDances(PackIds.Ballroom), x => x.Canonical, (x, t) => x.Check(t));
    [Fact] public void StreetLatinDanceAttrs_MatchRuntime() => AssertAttrs(Dances.StreetLatin.All, PackDances(PackIds.StreetLatin), x => x.Canonical, (x, t) => x.Check(t));

    [Fact] public void Genres_MatchRuntime() => AssertCatalog(Genres.All, Dim("genre"), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void Moods_MatchRuntime() => AssertCatalog(Moods.All, Dim("mood"), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void RhythmicFeels_MatchRuntime() => AssertCatalog(RhythmicFeels.All, Dim("rhythmic_feel"), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void Vocalities_MatchRuntime() => AssertCatalog(Vocalities.All, Dim("vocality"), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void Instrumentations_MatchRuntime() => AssertCatalog(Instrumentations.All, Dim("instrumentation"), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void Instruments_MatchRuntime() => AssertCatalog(Instruments.All, Dim("instrument"), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void RecordingContexts_MatchRuntime() => AssertCatalog(RecordingContexts.All, Dim("recording_context"), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void RecordingQualityEras_MatchRuntime() => AssertCatalog(RecordingQualityEras.All, Dim("recording_quality_era"), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void Themes_MatchRuntime() => AssertCatalog(Themes.All, Dim("theme"), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void SongForms_MatchRuntime() => AssertCatalog(SongForms.All, Dim("song_form"), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));

    [Fact] public void SwingDances_MatchRuntime() => AssertCatalog(Dances.Swing.All, PackDances(PackIds.Swing), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void BluesDances_MatchRuntime() => AssertCatalog(Dances.Blues.All, PackDances(PackIds.Blues), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void TangoDances_MatchRuntime() => AssertCatalog(Dances.Tango.All, PackDances(PackIds.Tango), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void BallroomDances_MatchRuntime() => AssertCatalog(Dances.Ballroom.All, PackDances(PackIds.Ballroom), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));
    [Fact] public void StreetLatinDances_MatchRuntime() => AssertCatalog(Dances.StreetLatin.All, PackDances(PackIds.StreetLatin), x => (x.Code.Value, x.Canonical, x.Aliases, x.Description));

    // The widened mood axes: every catalog mood carries a 1–10 energy×valence coordinate and the level
    // type orders ordinally. Guards the range + type contract only — placement is hand-owned and free to
    // re-tune (a moved value stays in range), so this deliberately does NOT lock specific coordinates.
    [Fact]
    public void Moods_carry_ordinal_1to10_coordinates()
    {
        foreach (var m in Moods.All)
        {
            Assert.NotNull(m.Energy);
            Assert.NotNull(m.Valence);
            Assert.InRange(m.Energy!.Value.Value, 1, 10);
            Assert.InRange(m.Valence!.Value.Value, 1, 10);
        }

        Assert.True(new EnergyLevel(3).CompareTo(new EnergyLevel(7)) < 0);
        Assert.Equal("8", new ValenceLevel(8).ToString());
    }
}
