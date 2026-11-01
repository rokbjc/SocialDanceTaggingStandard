using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class DisplayOverlayTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();

    [Fact]
    public void LoadDefault_SurfacesColor_AndNullShortLabel_WhenNoLabelAuthored()
    {
        var term = Vocab.Dimension(DimensionIds.Genre)!.Resolve("Chicago Blues")!;
        Assert.NotNull(term.Display);
        Assert.False(string.IsNullOrEmpty(term.Display!.Color));   // every term carries a colour (authored or axis-derived)
        Assert.Null(term.Display!.ShortLabel);                     // loader reads `label`; none authored in the baseline yet
    }

    [Fact]
    public void AuthoredDanceColors_Surface_AndAreDistinctWithinTheSwingCategory()
    {
        var swing = Vocab.Dances(PackIds.Swing);
        Assert.Equal("#E6433B", swing.Resolve("Lindy Hop")!.Display!.Color);   // authored suggested colour wins over the derived tradition hue

        var colors = swing.Terms.Select(t => t.Display!.Color).ToList();
        Assert.All(colors, c => Assert.False(string.IsNullOrEmpty(c)));
        Assert.Equal(colors.Count, colors.Distinct(StringComparer.OrdinalIgnoreCase).Count());   // distinct within the category
    }
}
