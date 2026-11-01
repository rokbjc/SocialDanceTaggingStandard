using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>
/// The <see cref="Dimension"/> construction contract: one case-insensitive lookup carries canonicals,
/// aliases, and display codes, so a token two terms claim would leave one of them unreachable. Every
/// such collision is rejected except the one with a decidable winner — a display code yielding to
/// another term's explicit canonical or alias, which is how a personal overlay rebinds a code.
/// </summary>
/// <remarks><c>scripts/validate_data.py</c> is stricter on the shipped baseline: there not even the
/// rebinding is allowed, and a term repeating one of its own tokens (dead data that still resolves,
/// so nothing here can see it) fails too.</remarks>
public sealed class DimensionCollisionTests
{
    private static readonly DimensionId Dim = new("test");

    [Fact]
    public void TwoDisplayCodes_DifferingOnlyInCase_AreRejected()
    {
        var ex = Assert.Throws<SdtsVocabularyException>(() => new Dimension(Dim, "core",
        [
            Term("rockabilly-combo", "Rockabilly Combo", [], "RbC"),
            Term("rb-combo", "R&B Combo", [], "RBC"),
        ]));

        Assert.Contains("rb-combo", ex.Message, StringComparison.Ordinal);
        Assert.Contains("rockabilly-combo", ex.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("RBC", ex.Message, StringComparison.Ordinal);   // an overlay's token is the DJ's own text
    }

    [Fact]
    public void AnAlias_CollidingWithAnotherTermsCanonical_IsRejected()
    {
        Assert.Throws<SdtsVocabularyException>(() => new Dimension(Dim, "core",
        [
            Term("salsa", "Salsa", []),
            Term("bachata", "Bachata", ["SALSA"]),
        ]));
    }

    [Fact]
    public void ATermId_DefinedTwice_IsRejected()
    {
        Assert.Throws<SdtsVocabularyException>(() => new Dimension(Dim, "core",
        [
            Term("swing", "Swing", []),
            Term("swing", "My Swing", []),
        ]));
    }

    [Fact]
    public void ADisplayCode_YieldsToAnotherTermsAlias()
    {
        var dim = new Dimension(Dim, "core",
        [
            Term("st-louis-shag", "St. Louis Shag", [], "SLS"),
            Term("salsa", "Salsa", ["SLS"]),
        ]);

        Assert.Equal("salsa", dim.Resolve("SLS")!.Id);
        Assert.Equal("st-louis-shag", dim.Resolve("St. Louis Shag")!.Id);
    }

    [Fact]
    public void ADisplayCode_MayRepeatItsOwnCanonical()
    {
        var dim = new Dimension(Dim, "core", [Term("pop", "Pop", [], "POP")]);

        Assert.Equal("pop", dim.Resolve("POP")!.Id);
    }

    [Fact]
    public void AnOverlayAlias_CollidingWithAnotherTerm_IsRejectedAtBuild()
    {
        var vocab = SocialDanceVocabulary.LoadDefault();

        Assert.Throws<SdtsVocabularyException>(
            () => vocab.Customize().AddAlias(DimensionIds.Genre, "Blues", "Swing").Build());
    }

    private static VocabularyTerm Term(string id, string canonical, string[] aliases, string? code = null)
        => new(id, canonical, aliases, false,
               new Dictionary<string, object?>(StringComparer.Ordinal),
               code is null ? null : new TermDisplay(code, null, null, null));
}
