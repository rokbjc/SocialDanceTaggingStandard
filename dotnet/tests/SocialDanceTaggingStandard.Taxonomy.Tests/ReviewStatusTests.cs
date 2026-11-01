using SocialDanceTaggingStandard.Taxonomy;
using SocialDanceTaggingStandard.Taxonomy.Profile;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>
/// The expert-review flags in data/vocabulary/ — a pack.yaml <c>validation</c> status and a core file's
/// <c>validation:</c> map — reach the API, and a vocabulary rebuilt by an overlay or a profile keeps them.
/// </summary>
public sealed class ReviewStatusTests
{
    private static readonly SocialDanceVocabulary Baseline = SocialDanceVocabulary.LoadDefault();

    [Fact]
    public void Baseline_FlagsPacksAndGenreAttributesAwaitingReview()
    {
        Assert.Equal(ReviewStatus.ExpertReviewPending, Baseline.Packs[PackIds.Tango].ReviewStatus);
        Assert.Equal(ReviewStatus.Unflagged, Baseline.Packs[PackIds.Swing].ReviewStatus);

        var review = Baseline.Dimension(DimensionIds.Genre)!.AttributeReview;
        Assert.Equal(ReviewStatus.ExpertReviewPending, review.GetValueOrDefault("typical_rhythmic_feels"));
        Assert.Equal(ReviewStatus.ExpertReviewPending, review.GetValueOrDefault("typical_song_forms"));
        Assert.Equal(ReviewStatus.Unflagged, review.GetValueOrDefault("era_start"));
    }

    [Fact]
    public void Overlay_KeepsReviewFlags()
    {
        var overlay = new VocabularyOverlay()
            .AddAlias(DimensionIds.Genre, "Chicago Blues", "Chi Blues")
            .AddAlias(SocialDanceVocabulary.DancesDimension, "Tango (Argentino)", "My Tango");

        AssertSameFlags(Baseline, Baseline.With(overlay));
    }

    [Fact]
    public void Profile_KeepsReviewFlags()
    {
        var profile = new PersonalProfile(colors:
        [
            new ColorOverride("genre", "Chicago Blues", "#1F3A93"),
            new ColorOverride("dances", "Tango (Argentino)", "#000000"),
        ]);

        AssertSameFlags(Baseline, Baseline.WithProfile(profile).Vocabulary);
    }

    private static void AssertSameFlags(SocialDanceVocabulary expected, SocialDanceVocabulary actual)
    {
        foreach (var (id, pack) in expected.Packs)
            Assert.Equal(pack.ReviewStatus, actual.Packs[id].ReviewStatus);
        foreach (var (id, dimension) in expected.Core)
        {
            Assert.Equal(
                dimension.AttributeReview.OrderBy(kv => kv.Key, StringComparer.Ordinal),
                actual.Core[id].AttributeReview.OrderBy(kv => kv.Key, StringComparer.Ordinal));
        }
    }
}
