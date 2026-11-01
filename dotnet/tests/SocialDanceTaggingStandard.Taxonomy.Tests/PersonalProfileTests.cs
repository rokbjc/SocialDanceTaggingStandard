using SocialDanceTaggingStandard.Internal;
using SocialDanceTaggingStandard.Sync;     // ISdtsSource (zero-write spy)
using SocialDanceTaggingStandard.Taxonomy;
using SocialDanceTaggingStandard.Taxonomy.Profile;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class PersonalProfileTests
{
    private static readonly SocialDanceVocabulary Baseline = SocialDanceVocabulary.LoadDefault();

    private static PersonalProfile SampleProfile()
    {
        var overlay = new VocabularyOverlay()
            .AddTerm(DimensionIds.Genre, new VocabularyTerm("user-balkan-brass", "Balkan Brass", ["Brass BG"], false,
                new Dictionary<string, object?>()));
        return new PersonalProfile(
            overlay,
            colors: [new ColorOverride("genre", "Chicago Blues", "#1F3A93", "Chi-Blues")],
            crossDimension: [new CrossDimensionRule("genre", "Slovenian", "artist_country", "SI", KeepSource: true)],
            owner: "rok");
    }

    // ── WithProfile: overlay + colors + remaps; baseline immutable ──────────────

    [Fact]
    public void WithProfile_AppliesOverlayAndColors_AndLeavesBaselineImmutable()
    {
        var baselineGenreCount = Baseline.Dimension(DimensionIds.Genre)!.Terms.Count;

        var cz = Baseline.WithProfile(SampleProfile());

        Assert.True(cz.Report.IsValid, string.Join("; ", cz.Report.Errors));
        Assert.NotNull(cz.Vocabulary.Dimension(DimensionIds.Genre)!.Resolve("Balkan Brass"));   // custom term added
        Assert.NotNull(cz.Vocabulary.Dimension(DimensionIds.Genre)!.Resolve("Brass BG"));        // its alias resolves
        var recolored = cz.Vocabulary.Dimension(DimensionIds.Genre)!.Resolve("Chicago Blues")!;
        Assert.Equal("#1F3A93", recolored.Display!.Color);                            // color override applied
        Assert.Equal("Chi-Blues", recolored.Display!.ShortLabel);                     // label override applied

        // baseline untouched
        Assert.Equal(baselineGenreCount, Baseline.Dimension(DimensionIds.Genre)!.Terms.Count);
        Assert.Null(Baseline.Dimension(DimensionIds.Genre)!.Resolve("Balkan Brass"));
    }

    // ── Project (read-time) ─────────────────────────────────────────────────────

    [Fact]
    public void Project_CrossDimensionKeep_AndSameDimensionAlias()
    {
        var mapper = Baseline.WithProfile(SampleProfile()).Mapper;

        var result = mapper.Project(new Dictionary<string, IReadOnlyList<string>>
        {
            ["dances"] = ["LH"],           // baseline display code of Lindy Hop
            ["genre"] = ["Slovenian"],     // cross-dim → artist_country SI, source kept
        });

        Assert.Equal(["Lindy Hop"], result.Projected["dances"]);     // alias → canonical
        Assert.Equal(["SI"], result.Projected["artist_country"]);    // cross-dim projected
        Assert.Equal(["Slovenian"], result.Projected["genre"]);      // source kept (on_source: keep)
    }

    [Fact]
    public void Project_DropsSource_WhenKeepSourceFalse()
    {
        var profile = new PersonalProfile(crossDimension:
            [new CrossDimensionRule("genre", "Slovenian", "artist_country", "SI", KeepSource: false)]);
        var mapper = Baseline.WithProfile(profile).Mapper;

        var result = mapper.Project(new Dictionary<string, IReadOnlyList<string>> { ["genre"] = ["Slovenian"] });

        Assert.Equal(["SI"], result.Projected["artist_country"]);
        Assert.False(result.Projected.ContainsKey("genre"));   // source dropped
    }

    [Fact]
    public void Project_UnknownValue_PassesThroughVerbatim()
    {
        var mapper = Baseline.WithProfile(new PersonalProfile()).Mapper;

        var result = mapper.Project(new Dictionary<string, IReadOnlyList<string>> { ["genre"] = ["Totally Made Up"] });

        Assert.Equal(["Totally Made Up"], result.Projected["genre"]);   // never lost
    }

    // ── Hash safety ─────────────────────────────────────────────────────────────

    [Fact]
    public void CustomTerm_HashesBareCanonical_NotTheUserId()
    {
        var cz = Baseline.WithProfile(SampleProfile());
        var term = cz.Vocabulary.Dimension(DimensionIds.Genre)!.Resolve("Balkan Brass")!;

        Assert.Equal("Balkan Brass", term.Canonical);                                  // id is user-balkan-brass; canonical is bare
        Assert.Equal(StableHash.Of("Balkan Brass"), StableHash.Of(term.Canonical));
        Assert.NotEqual(StableHash.Of("user-balkan-brass"), StableHash.Of(term.Canonical));
    }

    [Fact]
    public void DisplayLabelOverride_DoesNotChangeHashedCanonical()
    {
        // a "rename" is only a display-label override — the canonical (the one hashed thing) never moves
        var profile = new PersonalProfile(colors: [new ColorOverride("dances", "Balboa", Label: "Bal")]);
        var dances = Baseline.WithProfile(profile).Vocabulary.Dances(PackIds.Swing, PackIds.Blues);
        var term = dances.Resolve("Balboa")!;

        Assert.Equal("Balboa", term.Canonical);                       // unchanged
        Assert.Equal("Bal", term.Display!.ShortLabel);                // label overridden
        Assert.Equal(StableHash.Of("Balboa"), StableHash.Of(term.Canonical));
    }

    // ── Migration plan (write-time) ─────────────────────────────────────────────

    [Fact]
    public void PlanMigration_WritesBareCanonical_ThatHashesStably_AndRemovesDroppedSource()
    {
        var profile = new PersonalProfile(crossDimension:
            [new CrossDimensionRule("genre", "Slovenian", "artist_country", "SI", KeepSource: false, Migrate: true)]);

        var plan = Baseline.WithProfile(profile).Mapper.PlanMigration(
            new Dictionary<string, IReadOnlyList<string>> { ["genre"] = ["Slovenian"] });

        var write = Assert.Single(plan.Writes);
        Assert.Equal("artist_country", write.Dimension);
        Assert.Equal("SI", write.Value);
        Assert.Equal(StableHash.Of("SI"), StableHash.Of(write.Value));   // hashes as a native-canonical write
        var removal = Assert.Single(plan.Removals);
        Assert.Equal("genre", removal.Dimension);
        Assert.Equal("Slovenian", removal.Value);
    }

    [Fact]
    public void PlanMigration_IgnoresMigrateFalseRules()
    {
        var profile = new PersonalProfile(crossDimension:
            [new CrossDimensionRule("genre", "Slovenian", "artist_country", "SI", Migrate: false)]);

        var plan = Baseline.WithProfile(profile).Mapper.PlanMigration(
            new Dictionary<string, IReadOnlyList<string>> { ["genre"] = ["Slovenian"] });

        Assert.Empty(plan.Writes);
        Assert.Empty(plan.Removals);
    }

    // ── Zero-write projection ───────────────────────────────────────────────────

    private sealed class SpyTagSource(Dictionary<string, string?> data) : ISdtsSource
    {
        public int SetCalls { get; private set; }
        public string? Get(TagKey key) => data.TryGetValue(key.Value, out var v) ? v : null;
        public void Set(TagKey key, string? value) => SetCalls++;
    }

    [Fact]
    public void Project_PerformsNoWrites_OnTheSource()
    {
        var source = new SpyTagSource(new Dictionary<string, string?> { ["genre"] = "Slovenian", ["dances"] = "#LH" });
        var mapper = Baseline.WithProfile(SampleProfile()).Mapper;

        var tags = new Dictionary<string, IReadOnlyList<string>>
        {
            ["genre"] = [source.Get(new TagKey("genre"))!],
            ["dances"] = [source.Get(new TagKey("dances"))!],
        };
        _ = mapper.Project(tags);

        Assert.Equal(0, source.SetCalls);   // read-time projection never writes
    }

    // ── Governance ──────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_UnprefixedCustomId_IsError()
    {
        var overlay = new VocabularyOverlay()
            .AddTerm(DimensionIds.Genre, new VocabularyTerm("balkanbrass", "Balkan Brass", [], false, new Dictionary<string, object?>()));

        var report = ProfileValidator.Validate(Baseline, new PersonalProfile(overlay));

        Assert.False(report.IsValid);
        Assert.Contains(report.Errors, e => e.Contains("balkanbrass"));
    }

    [Fact]
    public void Validate_CustomCanonicalCollidingWithBaseline_IsError()
    {
        var overlay = new VocabularyOverlay()
            .AddTerm(DimensionIds.Genre, new VocabularyTerm("user-swing", "Swing", [], false, new Dictionary<string, object?>()));

        var report = ProfileValidator.Validate(Baseline, new PersonalProfile(overlay));

        Assert.False(report.IsValid);
        Assert.Contains(report.Errors, e => e.Contains("collides"));
    }

    [Fact]
    public void Validate_AliasResolvingToTwoCanonicals_IsError()
    {
        // adding the existing canonical "Swing" as an alias of a DIFFERENT term is ambiguous
        var overlay = new VocabularyOverlay().AddAlias(DimensionIds.Genre, "Blues", "Swing");

        var report = ProfileValidator.Validate(Baseline, new PersonalProfile(overlay));

        Assert.False(report.IsValid);
        Assert.Contains(report.Errors, e => e.Contains("already resolves"));
    }

    [Fact]
    public void Validate_DanglingCrossDimensionTarget_IsWarning_AndRuleDropped()
    {
        var profile = new PersonalProfile(crossDimension:
            [new CrossDimensionRule("genre", "Slovenian", "mood", "NotARealMood")]);   // mood is a vocab dim; value doesn't resolve

        var cz = Baseline.WithProfile(profile);

        Assert.Contains(cz.Report.Warnings, w => w.Contains("NotARealMood"));
        var result = cz.Mapper.Project(new Dictionary<string, IReadOnlyList<string>> { ["genre"] = ["Slovenian"] });
        Assert.False(result.Projected.ContainsKey("mood"));   // dangling rule dropped from the mapper
    }

    [Fact]
    public void WithProfile_CollidingAlias_IsDropped_AndTheRestOfTheProfileApplies()
    {
        // "Swing" is another genre's canonical, so this alias could never resolve to Blues. The other
        // alias in the same profile is fine and must survive it.
        var overlay = new VocabularyOverlay()
            .AddAlias(DimensionIds.Genre, "Blues", "Swing")
            .AddAlias(DimensionIds.Mood, "Joyful", "Sunshiney");

        var cz = Baseline.WithProfile(new PersonalProfile(overlay));

        Assert.False(cz.Report.IsValid);
        Assert.Contains(cz.Report.Errors, e => e.Contains("dropped"));
        Assert.Equal("Blues", cz.Vocabulary.Dimension(DimensionIds.Genre)!.Resolve("Blues")!.Canonical);
        Assert.Equal("Swing", cz.Vocabulary.Dimension(DimensionIds.Genre)!.Resolve("Swing")!.Canonical);
        Assert.Equal("Joyful", cz.Vocabulary.Dimension(DimensionIds.Mood)!.Resolve("Sunshiney")!.Canonical);
    }

    [Fact]
    public void WithProfile_CustomTermWhoseAliasCollides_IsDropped_NotThrown()
    {
        // The alias, not the canonical — the governance rules only check a custom term's canonical, so
        // this is the case only the apply step can catch.
        var overlay = new VocabularyOverlay()
            .AddTerm(DimensionIds.Genre, new VocabularyTerm("user-my-swing", "My Swing", ["Swing"], false,
                new Dictionary<string, object?>()));

        var cz = Baseline.WithProfile(new PersonalProfile(overlay));

        Assert.Contains(cz.Report.Errors, e => e.Contains("user-my-swing"));
        Assert.Equal("Swing", cz.Vocabulary.Dimension(DimensionIds.Genre)!.Resolve("Swing")!.Canonical);
        Assert.Null(cz.Vocabulary.Dimension(DimensionIds.Genre)!.Resolve("My Swing"));
    }

}
