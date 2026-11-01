using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class VocabularyCustomizationTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();

    [Fact]
    public void Customize_AddAlias_RemoveAlias_AndLeaveBaselineUntouched()
    {
        var custom = Vocab.Customize()
            .AddAlias(DimensionIds.Genre, "Swing", "My Swing")
            .AddAlias(SocialDanceVocabulary.DancesDimension, "Lindy Hop", "#LH")  // personal '#' shorthand
            .RemoveAlias(SocialDanceVocabulary.DancesDimension, "Lindy")          // opt out of a pack alias
            .Build();

        Assert.Equal("Swing", custom.Dimension(DimensionIds.Genre)!.Resolve("My Swing")!.Canonical);
        // An overlay reaches PACK dance terms, not just core dimensions — what a DJ's personal '#' codes
        // depend on now that they are no longer baseline.
        Assert.Equal("Lindy Hop", custom.Packs[PackIds.Swing].Dances.Resolve("#LH")!.Canonical);
        Assert.Null(custom.Packs[PackIds.Swing].Dances.Resolve("Lindy"));         // removed
        Assert.NotNull(custom.Packs[PackIds.Swing].Dances.Resolve("Lindy Hop"));  // canonical still resolves

        // The baseline is immutable — unaffected by the customization.
        Assert.Null(Vocab.Dimension(DimensionIds.Genre)!.Resolve("My Swing"));
        Assert.Null(Vocab.Packs[PackIds.Swing].Dances.Resolve("#LH"));            // '#' is not baseline
        Assert.NotNull(Vocab.Packs[PackIds.Swing].Dances.Resolve("Lindy"));
    }

    [Fact]
    public void Customize_AddTerm_ToCoreDimension()
    {
        var term = new VocabularyTerm("balkan-brass", "Balkan Brass", ["Brass BG"],
            false, new Dictionary<string, object?>());
        var custom = Vocab.Customize().AddTerm(DimensionIds.Genre, term).Build();

        Assert.Equal("Balkan Brass", custom.Dimension(DimensionIds.Genre)!.Resolve("Brass BG")!.Canonical);
        Assert.Null(Vocab.Dimension(DimensionIds.Genre)!.Resolve("Brass BG")); // baseline unchanged
    }

    [Fact]
    public void SaveOverlay_PersistsCustomValue_AndSurvivesLoadOverlay()
    {
        // The WRITE counterpart to LoadOverlay: a custom value (+ alias) added to an overlay, saved, then
        // reloaded, is present — the persistence a per-DJ personal-value registry builds on.
        var overlay = new VocabularyOverlay()
            .AddTerm(DimensionIds.Genre, new VocabularyTerm("balkan-brass", "Balkan Brass", ["Brass BG"],
                false, new Dictionary<string, object?>()))
            .AddAlias(DimensionIds.Mood, "Joyful", "Stoked");

        var file = new FileInfo(Path.Combine(Path.GetTempPath(), $"sdts-overlay-{Guid.NewGuid():N}.yaml"));
        try
        {
            SocialDanceVocabulary.SaveOverlay(overlay, file);
            var custom = Vocab.With(SocialDanceVocabulary.LoadOverlay(file));

            Assert.Equal("Balkan Brass", custom.Dimension(DimensionIds.Genre)!.Resolve("Balkan Brass")!.Canonical);
            Assert.Equal("Balkan Brass", custom.Dimension(DimensionIds.Genre)!.Resolve("Brass BG")!.Canonical); // alias survived
            Assert.Equal("Joyful", custom.Dimension(DimensionIds.Mood)!.Resolve("Stoked")!.Canonical);          // added alias survived
            Assert.Null(Vocab.Dimension(DimensionIds.Genre)!.Resolve("Brass BG"));                              // baseline untouched
        }
        finally
        {
            file.Refresh();
            if (file.Exists) file.Delete();
        }
    }

    [Fact]
    public void LoadOverlay_FromYaml_AppliesAllOps()
    {
        const string yaml = @"
add_aliases:
  mood:
    Joyful: [Sunshiney]
  dances:
    Balboa: ['#BLB']
add_terms:
  genre:
    - {id: balkan-brass, canonical: Balkan Brass, aliases: [Brass BG]}
";
        var overlay = SocialDanceVocabulary.LoadOverlay(new StringReader(yaml));
        var custom = Vocab.With(overlay);

        Assert.Equal("Joyful", custom.Dimension(DimensionIds.Mood)!.Resolve("Sunshiney")!.Canonical);
        Assert.Equal("Balboa", custom.Packs[PackIds.Swing].Dances.Resolve("#BLB")!.Canonical);
        Assert.Equal("Balkan Brass", custom.Dimension(DimensionIds.Genre)!.Resolve("Brass BG")!.Canonical);
    }
}
