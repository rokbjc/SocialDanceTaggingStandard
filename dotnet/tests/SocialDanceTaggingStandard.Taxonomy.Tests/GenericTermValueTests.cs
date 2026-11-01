using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class GenericTermValueTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();

    // ── Value model ────────────────────────────────────────────────────────────

    [Fact]
    public void GenreValue_Known_CarriesCatalogTerm_AndStoredText()
    {
        var v = new GenreValue(Genres.Gospel, "Black Gospel");
        Assert.True(v.IsKnown);
        Assert.Same(Genres.Gospel, v.Term);     // identity preserved (the point of B2)
        Assert.Equal("Gospel", v.Canonical);
        Assert.Equal("Black Gospel", v.StoredValue);
        Assert.Equal(ValueOrigin.Stored, v.Origin);
    }

    [Fact]
    public void GenreValue_Generic_IsNotKnown_AndCanonicalFallsBackToStored()
    {
        var v = new GenreValue(Genres.Generic, "Foobar");
        Assert.False(v.IsKnown);
        Assert.Equal("Foobar", v.Canonical);     // generic has no canonical → stored text
        Assert.Equal("Foobar", v.StoredValue);
    }

    [Fact]
    public void Genres_ById_MapsStableId_ToConstant()
    {
        Assert.Same(Genres.Gospel, Genres.ById["gospel"]);
    }

    [Fact]
    public void GenreField_HoldsValuesAndRawFrame()
    {
        var f = new GenreField([new GenreValue(Genres.Generic, "x")], "x");
        Assert.Single(f.Values);
        Assert.Equal("x", f.RawFrame);
    }

    // ── Resolution (instance-bound) ─────────────────────────────────────────────

    [Fact]
    public void Resolve_Alias_ResolvesToCanonical_AndKeepsStoredText()
    {
        var v = Vocab.Genre().Resolve("Black Gospel");   // alias of Gospel
        Assert.True(v.IsKnown);
        Assert.Same(Genres.Gospel, v.Term);
        Assert.Equal("Black Gospel", v.StoredValue);
    }

    [Fact]
    public void Resolve_Unknown_ReturnsGeneric_CarryingRawText()
    {
        var v = Vocab.Genre().Resolve("Totally Made Up");
        Assert.False(v.IsKnown);
        Assert.Same(Genres.Generic, v.Term);
        Assert.Equal("Totally Made Up", v.StoredValue);
    }

    [Fact]
    public void Read_MultiValueFrame_SplitsAndPreservesRawFrame()
    {
        var f = Vocab.Genre().Read("Swing; Foobar");
        Assert.Equal(2, f.Values.Count);
        Assert.True(f.Values[0].IsKnown);             // Swing matched
        Assert.False(f.Values[1].IsKnown);            // Foobar generic
        Assert.Equal("Foobar", f.Values[1].StoredValue);
        Assert.Equal("Swing; Foobar", f.RawFrame);    // original preserved
    }

    [Fact]
    public void Read_EmptyFrame_YieldsNoValues()
    {
        var f = Vocab.Genre().Read("");
        Assert.Empty(f.Values);
        Assert.Equal("", f.RawFrame);
    }

    [Fact]
    public void Resolve_OverlayAddedAlias_ResolvesAsKnown_ViaInstanceBoundView()
    {
        var vocab = Vocab.Customize().AddAlias(DimensionIds.Genre, "Swing", "Le Swing").Build();
        var v = vocab.Genre().Resolve("Le Swing");
        Assert.True(v.IsKnown);                        // customization honored by the instance-bound view
        Assert.Equal("Swing", v.Term.Canonical);
        Assert.Equal("Le Swing", v.StoredValue);
    }

    // ── Drift gate ──────────────────────────────────────────────────────────────

    [Fact]
    public void ById_CoversEveryRuntimeGenreTerm()
    {
        var dim = Vocab.Dimension(DimensionIds.Genre)!;
        foreach (var term in dim.Terms)
        {
            Assert.True(Genres.ById.ContainsKey(term.Id),
                $"Genres.ById missing id '{term.Id}' — regenerate gen_vocab_docs.py");
            Assert.Equal(term.Canonical, Genres.ById[term.Id].Canonical);
        }
    }

    [Fact]
    public void Generic_IsDistinctFromEveryCatalogTerm()
    {
        Assert.DoesNotContain(Genres.All, g => ReferenceEquals(g, Genres.Generic));
        Assert.False(new GenreValue(Genres.Gospel, "x").Equals(new GenreValue(Genres.Generic, "x")));
    }
}
