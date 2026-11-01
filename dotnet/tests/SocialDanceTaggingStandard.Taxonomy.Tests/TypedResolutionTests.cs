using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class TypedResolutionTests
{
    [Fact]
    public void ResolveGenres_MapsCanonical_AndKeepsUnknown()
    {
        var vocab = SocialDanceVocabulary.LoadDefault();
        var genres = vocab.ResolveGenres(["Swing", "Frobnitz"]);

        Assert.Equal(2, genres.Count);
        Assert.Equal("Swing", genres[0].Canonical);

        var known = vocab.ResolveKnownGenres(["Swing", "Frobnitz"]);
        Assert.Single(known);           // unknown skipped in the *Known variant
    }

    [Fact]
    public void ResolveDances_ResolvesAlias()
    {
        var vocab = SocialDanceVocabulary.LoadDefault();
        var dances = vocab.ResolveDances(["Lindy"]);   // alias of Lindy Hop
        Assert.Equal("Lindy Hop", dances[0].Canonical);
    }
}
