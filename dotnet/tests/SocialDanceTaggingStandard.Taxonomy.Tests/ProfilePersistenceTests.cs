using SocialDanceTaggingStandard.Taxonomy;
using SocialDanceTaggingStandard.Taxonomy.Ingest;
using SocialDanceTaggingStandard.Taxonomy.Profile;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>The <c>.sdvo</c> format extended to a full <see cref="PersonalProfile"/> (SP2 point 2): the
/// vocabulary overlay PLUS the read-time rule sections (cross_dimension, colors, from_path) persist and
/// round-trip, at schemaVersion 2, and an older overlay-only file still loads as a profile with no rules.</summary>
public sealed class ProfilePersistenceTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();

    private static PersonalProfile RoundTrip(PersonalProfile profile)
    {
        var writer = new StringWriter();
        SocialDanceVocabulary.SaveProfile(profile, writer);
        return SocialDanceVocabulary.LoadProfile(new StringReader(writer.ToString()));
    }

    [Fact]
    public void FullProfile_RoundTrips_EveryRuleSection()
    {
        var overlay = new VocabularyOverlay().AddAlias(SocialDanceVocabulary.DancesDimension, "Lindy Hop", "#LH");
        var cross = new CrossDimensionRule("genre", "Slovenian", "artist_country", "SI",
            KeepSource: false, Migrate: true, Match: MatchMode.Exact);
        var color = new ColorOverride("mood", "Joyful", "#FFCC00", "Happy");
        var path = new PathRule(
            "**/*.mp3",
            "%artist% - %title% <tail>",
            [new PathCode("blb", "dances", "Balboa")],
            new PlusRunRating("rating", RatingScale.Linear, 5),
            new YearExtractor("recording_year", @"\d{4}"),
            UnknownTokenPolicy.Keep)
        {
            Set = new Dictionary<string, string> { ["genre"] = "Swing" },
        };
        var profile = new PersonalProfile(overlay, [color], [cross], "rok", [path]);

        var back = RoundTrip(profile);

        Assert.Equal("rok", back.Owner);
        Assert.Equal(cross, Assert.Single(back.CrossDimension));                 // record value-equality (all scalar members)
        Assert.Equal(color, Assert.Single(back.Colors));
        Assert.Equal("Lindy Hop",
            Vocab.With(back.Overlay).Packs[PackIds.Swing].Dances.Resolve("#LH")!.Canonical);  // overlay survived

        var p = Assert.Single(back.FromPath);
        Assert.Equal("**/*.mp3", p.Match);
        Assert.Equal("%artist% - %title% <tail>", p.Mask);
        Assert.Equal(new PathCode("blb", "dances", "Balboa"), Assert.Single(p.Codes));
        Assert.Equal(new PlusRunRating("rating", RatingScale.Linear, 5), p.Rating);
        Assert.Equal(new YearExtractor("recording_year", @"\d{4}"), p.Year);
        Assert.Equal(UnknownTokenPolicy.Keep, p.OnUnknownToken);
        Assert.Equal("Swing", p.Set["genre"]);
    }

    [Fact]
    public void SavedProfile_IsVersion2AndSelfDescribing()
    {
        var profile = new PersonalProfile(crossDimension: [new CrossDimensionRule("genre", "X", "theme", "Y")]);
        var writer = new StringWriter();
        SocialDanceVocabulary.SaveProfile(profile, writer);
        var text = writer.ToString();

        Assert.Equal($"# yaml-language-server: $schema={SdvoFormat.SchemaId}", text.Split('\n')[0].TrimEnd('\r'));
        Assert.Contains("schemaVersion: 2", text, StringComparison.Ordinal);
        Assert.Equal(2, SdvoFormat.CurrentVersion);
    }

    [Fact]
    public void CrossDimensionRule_AllDefaults_RoundTripsThroughDefaults()
    {
        var rule = new CrossDimensionRule("genre", "Slovenian", "artist_country", "SI"); // KeepSource=true, Migrate=false, CaseInsensitive
        var back = Assert.Single(RoundTrip(new PersonalProfile(crossDimension: [rule])).CrossDimension);

        Assert.Equal(rule, back);
        Assert.True(back.KeepSource);
        Assert.False(back.Migrate);
        Assert.Equal(MatchMode.CaseInsensitive, back.Match);
    }

    [Fact]
    public void OverlayOnlyFile_LoadsAsAProfile_WithEmptyRuleSections()
    {
        var overlay = new VocabularyOverlay().AddAlias(DimensionIds.Mood, "Joyful", "Sunshiney");
        var writer = new StringWriter();
        SocialDanceVocabulary.SaveOverlay(overlay, writer);

        var profile = SocialDanceVocabulary.LoadProfile(new StringReader(writer.ToString()));

        Assert.Empty(profile.CrossDimension);
        Assert.Empty(profile.Colors);
        Assert.Empty(profile.FromPath);
        Assert.Equal("Joyful", Vocab.With(profile.Overlay).Dimension(DimensionIds.Mood)!.Resolve("Sunshiney")!.Canonical);
    }

    [Fact]
    public void LegacyUnversionedFile_LoadsAsAProfile_WithNoRules()
    {
        const string legacy = "add_aliases:\n  mood:\n    Joyful: [Sunshiney]\n";

        var profile = SocialDanceVocabulary.LoadProfile(new StringReader(legacy));

        Assert.Empty(profile.FromPath);
        Assert.Equal("Joyful", Vocab.With(profile.Overlay).Dimension(DimensionIds.Mood)!.Resolve("Sunshiney")!.Canonical);
    }
}
