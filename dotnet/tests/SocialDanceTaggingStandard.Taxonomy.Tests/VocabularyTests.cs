using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class VocabularyTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();

    [Fact]
    public void LoadDefault_ReadsVersionAndCoreDimensions()
    {
        Assert.False(string.IsNullOrWhiteSpace(Vocab.Version));
        // The shared music dimensions all live in core and are never partitioned.
        Assert.Contains(DimensionIds.Genre, Vocab.Core.Keys);
        Assert.Contains(DimensionIds.Mood, Vocab.Core.Keys);
        Assert.Contains(DimensionIds.RhythmicFeel, Vocab.Core.Keys);
        Assert.True(Vocab.Dimension(DimensionIds.Genre)!.Terms.Count >= 100);
        Assert.True(Vocab.Dimension(DimensionIds.Mood)!.Terms.Count >= 21);   // lower bound — the set grows (e.g. proposals)
    }

    [Fact]
    public void Core_ResolvesCanonicalAndAlias()
    {
        var genre = Vocab.Dimension(DimensionIds.Genre)!;
        Assert.NotNull(genre.Resolve("Swing"));

        var mood = Vocab.Dimension(DimensionIds.Mood)!;
        // "Happy" is an alias of the canonical mood "Joyful".
        Assert.Equal("Joyful", mood.Resolve("happy")!.Canonical);
    }

    [Fact]
    public void Packs_AreSwingAndBlues()
    {
        Assert.Contains(PackIds.Swing, Vocab.Packs.Keys);
        Assert.Contains(PackIds.Blues, Vocab.Packs.Keys);
        Assert.Equal(12, Vocab.Packs[PackIds.Swing].Dances.Terms.Count);
        Assert.Equal(8, Vocab.Packs[PackIds.Blues].Dances.Terms.Count);
    }

    [Fact]
    public void Dances_AreProseCanonical_AndResolveByBareDisplayCode()
    {
        var swing = Vocab.Packs[PackIds.Swing].Dances;

        var byName = swing.Resolve("Lindy Hop");
        var byCode = swing.Resolve("LH");
        Assert.NotNull(byName);
        Assert.Equal("Lindy Hop", byName!.Canonical);   // canonical is the prose name
        Assert.Same(byName, byCode);                     // the bare display code is a first-class token
        Assert.Equal("swing-partner", byName.Family);    // vocabulary attribute carried through

        // The '#'-prefixed form is personal DJ shorthand, NOT part of the baseline. It only resolves
        // for a deployment that adds it back via an overlay (see VocabularyCustomizationTests).
        Assert.Null(swing.Resolve("#LH"));
    }

    [Fact]
    public void EnabledDances_MergesPacksDeduped()
    {
        var dances = Vocab.EnabledDances(PackIds.Swing, PackIds.Blues);
        Assert.Equal(20, dances.Count);
        Assert.Contains(dances, d => d.Canonical == "Blues");   // "Blues Dancing" is now an alias
        Assert.Contains(dances, d => d.Canonical == "Lindy Hop");
    }

    [Fact]
    public void IsValid_ChecksMembership_AliasAware()
    {
        Assert.True(Vocab.IsValid(DimensionIds.Mood, "Happy"));        // alias of Joyful
        Assert.True(Vocab.IsValid(DimensionIds.Genre, "Gypsy Jazz"));
        Assert.False(Vocab.IsValid(DimensionIds.Genre, "Reggaeton"));  // not in the vocabulary
        Assert.False(Vocab.IsValid(new DimensionId("nope"), "anything"));    // unknown dimension
    }

    [Fact]
    public void Normalize_CollapsesAliases_AndDropsParentWhenChildPresent()
    {
        // "swing"/"Swing" dedup to one; "Swing" is an ancestor of "Big Band" so it is dropped.
        var result = Vocab.Normalize(DimensionIds.Genre, ["Swing", "swing", "Big Band"]);
        Assert.Equal(["Big Band"], result.Select(t => t.Canonical));
    }

    [Fact]
    public void Normalize_KeepsUnrelatedTerms()
    {
        var result = Vocab.Normalize(DimensionIds.Genre, ["Hot Jazz", "Chicago Blues"]);
        Assert.Equal(2, result.Count); // different branches — neither is the other's ancestor
    }

    [Fact]
    public void Display_LoadsBareCodeAxisColorAndVerifiedIcon()
    {
        var lindy = Vocab.Packs[PackIds.Swing].Dances.Resolve("Lindy Hop")!;
        Assert.NotNull(lindy.Display);
        Assert.Equal("LH", lindy.Display!.Code);            // bare — never "#LH"
        Assert.False(string.IsNullOrEmpty(lindy.Display.Emoji));
        Assert.Matches("^[a-z0-9-]+:[a-z0-9-]+$", lindy.Display.Icon!); // Iconify prefix:name (any set)
        Assert.Equal("#E6433B", lindy.Display.Color);        // authored suggested colour (overrides the tradition-derived hue)

        // a term with no authored colour still gets the axis-derived one (mood by energy×valence grid).
        Assert.Equal("#FFB300", Vocab.Dimension(DimensionIds.Mood)!.Resolve("Joyful")!.Display!.Color); // high × bright
    }

    [Fact]
    public void Display_InstrumentLed_UsesRealInstrumentRegistryCodeAndEmoji()
    {
        var trumpet = Vocab.Dimension(DimensionIds.Instrument)!.Resolve("Trumpet")!;
        Assert.Equal("tp", trumpet.Display!.Code);   // from the reference implementation's instrument registry
        Assert.Equal("🎺", trumpet.Display.Emoji);
    }

    [Fact]
    public void Dances_Normalize_StripsGradeMarkersAndDedups()
    {
        var dances = Vocab.Dances(PackIds.Swing, PackIds.Blues);
        var result = dances.Normalize(["Lindy Hop:edge?", "#LH", "Balboa"]);
        // "Lindy Hop:edge?" and the "#LH" alias both resolve to Lindy Hop → deduped.
        Assert.Equal(["Lindy Hop", "Balboa"], result.Select(t => t.Canonical));
        Assert.True(dances.Contains("Slow Drag")); // blues pack folded in
    }

    [Fact]
    public void AuthoredPacks_TangoAndBallroom_SplitTheTangoCollision()
    {
        Assert.Contains(PackIds.Tango, Vocab.Packs.Keys);
        Assert.Contains(PackIds.Ballroom, Vocab.Packs.Keys);

        var argentine = Vocab.Packs[PackIds.Tango].Dances.Resolve("Argentine Tango")!;
        var intl = Vocab.Packs[PackIds.Ballroom].Dances.Resolve("International Tango")!;
        Assert.Equal("tango-argentino", argentine.Id);
        Assert.Equal("tango-international", intl.Id);          // same word, distinct dances
        Assert.Equal(DanceTradition.ArgentineTango, Dances.ById[argentine.Id].Tradition);
        Assert.Equal(DanceTradition.BallroomInternationalStandard, Dances.ById[intl.Id].Tradition);
        Assert.NotNull(intl.Display);                          // authored display present
        Assert.Equal("#FA6A94", argentine.Display!.Color);
    }
}
