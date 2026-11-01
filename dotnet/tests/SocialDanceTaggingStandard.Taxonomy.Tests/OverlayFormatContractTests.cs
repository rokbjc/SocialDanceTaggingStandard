using System.Text.Json;
using SocialDanceTaggingStandard.Taxonomy;
using SocialDanceTaggingStandard.TestKit;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>The <c>.sdvo.yaml</c> overlay format's contract: a saved file describes itself, an older
/// file migrates forward, a newer one is refused rather than half-read, and the shipped schema cannot
/// drift from the constants the loader uses.</summary>
public sealed class OverlayFormatContractTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();

    [Fact]
    public void SavedOverlay_IsSelfDescribing()
    {
        var overlay = new VocabularyOverlay()
            .AddAlias(SocialDanceVocabulary.DancesDimension, "Lindy Hop", "#LH");

        var writer = new StringWriter();
        SocialDanceVocabulary.SaveOverlay(overlay, writer);
        var lines = writer.ToString().Split('\n');

        Assert.Equal($"# yaml-language-server: $schema={SdvoFormat.SchemaId}", lines[0].TrimEnd('\r'));
        Assert.Contains($"schemaVersion: {SdvoFormat.CurrentVersion}", writer.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void SavedOverlay_RoundTrips_ThroughItsOwnModelineAndVersion()
    {
        var overlay = new VocabularyOverlay()
            .AddAlias(SocialDanceVocabulary.DancesDimension, "Balboa", "#BLB");

        var writer = new StringWriter();
        SocialDanceVocabulary.SaveOverlay(overlay, writer);
        var reloaded = SocialDanceVocabulary.LoadOverlay(new StringReader(writer.ToString()));

        Assert.Equal("Balboa", Vocab.With(reloaded).Packs[PackIds.Swing].Dances.Resolve("#BLB")!.Canonical);
    }

    [Fact]
    public void LegacyUnversionedOverlay_MigratesForward()
    {
        // The unversioned shape written before the format existed. v0 -> v1 moved no key, so it still
        // applies; a golden fixture of the old shape is exactly what must keep working.
        const string legacy = "add_aliases:\n  mood:\n    Joyful: [Sunshiney]\n";

        var overlay = SocialDanceVocabulary.LoadOverlay(new StringReader(legacy));

        Assert.Equal("Joyful", Vocab.With(overlay).Dimension(DimensionIds.Mood)!.Resolve("Sunshiney")!.Canonical);
    }

    [Fact]
    public void FutureVersion_IsRefused_NotPartiallyRead()
    {
        var future = $"schemaVersion: {SdvoFormat.CurrentVersion + 1}\nadd_aliases:\n  mood:\n    Joyful: [Sunshiney]\n";

        var ex = Assert.Throws<SdtsVocabularyException>(
            () => SocialDanceVocabulary.LoadOverlay(new StringReader(future)));

        Assert.Contains(SdvoFormat.CurrentVersion.ToString(), ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NonNumericVersion_IsRefused()
        => Assert.Throws<SdtsVocabularyException>(
            () => SocialDanceVocabulary.LoadOverlay(new StringReader("schemaVersion: draft\n")));

    [Fact]
    public void ShippedSchema_MatchesTheLoaderConstants()
    {
        // The drift gate: the schema is generated from schemas/sdvo.schema.json.in + the vocabulary
        // manifest, and the loader's constants are generated beside it. If either is hand-edited apart
        // from the other, this fails.
        using var doc = JsonDocument.Parse(File.ReadAllText(ConformancePaths.Resolve("schemas/sdvo.schema.json")));
        var root = doc.RootElement;

        Assert.Equal(SdvoFormat.SchemaId, root.GetProperty("$id").GetString());
        Assert.Equal(SdvoFormat.CurrentVersion,
            root.GetProperty("properties").GetProperty("schemaVersion").GetProperty("const").GetInt32());

        var schemaDims = root.GetProperty("$defs").GetProperty("dimensionName").GetProperty("enum")
            .EnumerateArray().Select(e => e.GetString()!).ToList();
        Assert.Equal(SdvoFormat.DimensionNames, schemaDims);

        // Every dimension the schema names must actually exist in the baseline, plus the virtual one.
        foreach (var dim in schemaDims.Where(d => d != SocialDanceVocabulary.DancesDimension.Value))
            Assert.NotNull(Vocab.Dimension(new DimensionId(dim)));
    }

    [Fact]
    public void ShippedOverlayFiles_CarryTheModelineAndVersion()
    {
        // The reachability check: every *.sdvo.yaml the repo ships is a file a DJ copies, so a broken
        // modeline in one of them ships the mistake. (Instance validation against the schema itself runs
        // in scripts/validate_data.py, where the JSON-Schema validator lives.)
        var repo = new DirectoryInfo(ConformancePaths.Root);
        var overlays = repo.GetFiles($"*{SdvoFormat.Extension}", SearchOption.AllDirectories);

        Assert.NotEmpty(overlays);
        foreach (var file in overlays)
        {
            var lines = File.ReadAllLines(file.FullName);
            Assert.Equal($"# yaml-language-server: $schema={SdvoFormat.SchemaId}", lines[0]);
            Assert.Contains(lines, l => l.StartsWith($"schemaVersion: {SdvoFormat.CurrentVersion}", StringComparison.Ordinal));
        }
    }
}
