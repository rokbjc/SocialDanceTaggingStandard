using SocialDanceTaggingStandard.Sync;
using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class TaxonomyValueTranslatorTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();
    private static readonly IValueTranslator T = new TaxonomyValueTranslator(Vocab, "; ", PackIds.Swing, PackIds.Blues);

    [Fact]
    public void Dances_MirrorToCanonical_ResolvesDisplayCodes()
    {
        Assert.Equal("Lindy Hop", T.MirrorToCanonical(FieldIds.Dance, "LH"));
        Assert.Equal("Lindy Hop; Balboa", T.MirrorToCanonical(FieldIds.Dance, "LH; BAL"));
    }

    [Fact]
    public void Dances_CanonicalToMirror_EmitsDisplayCode()
    {
        // The mirror carries the short code, not the prose name: MediaMonkey's Occasion column is scanned
        // at a glance. Not the '#'-prefixed form either — that is personal shorthand, not the standard.
        Assert.Equal("LH", T.CanonicalToMirror(FieldIds.Dance, "Lindy Hop"));
        Assert.Equal("LH; BLS", T.CanonicalToMirror(FieldIds.Dance, "Lindy Hop; Blues"));
    }

    [Fact]
    public void Dances_HashCode_ResolvesOnlyViaAPersonalOverlay()
    {
        // A DJ whose library already carries '#'-coded values adds them back as aliases. Reads then
        // canonicalize again, while the mirror keeps emitting the standard's display code either way.
        Assert.Equal("#LH", T.MirrorToCanonical(FieldIds.Dance, "#LH"));   // unresolvable → verbatim

        var personal = new TaxonomyValueTranslator(
            Vocab.Customize().AddAlias(SocialDanceVocabulary.DancesDimension, "Lindy Hop", "#LH").Build(),
            "; ", PackIds.Swing, PackIds.Blues);

        Assert.Equal("Lindy Hop", personal.MirrorToCanonical(FieldIds.Dance, "#LH"));
        Assert.Equal("LH", personal.CanonicalToMirror(FieldIds.Dance, "Lindy Hop"));
    }

    [Fact]
    public void EveryCatalogBackingDimension_ResolvesToARealVocabularyDimension()
    {
        // The catalog's BackingDimension is the single source the translator now reads. Prove that every
        // non-null one actually names a dimension the loaded vocabulary can resolve values against — so a
        // future field whose backing drifts from the vocabulary fails here, not silently at a call site.
        foreach (var meta in SocialDanceTaggingStandard.FieldCatalog.All.Values)
        {
            if (meta.BackingDimension is not { } dim) continue;
            var resolvable = dim == SocialDanceVocabulary.DancesDimension.Value
                ? Vocab.Dances(PackIds.Swing, PackIds.Blues, PackIds.StreetLatin, PackIds.Ballroom, PackIds.Tango) is { }
                : Vocab.Dimension(new DimensionId(dim)) is not null;
            Assert.True(resolvable, $"field '{meta.Id.Value}' backs a non-existent dimension '{dim}'");
        }
    }

    [Fact]
    public void Dances_MirrorNeverEmitsACodeAnOverlayRebound()
    {
        // The reported case: bare 'SLS' is St. Louis Shag's display code in the baseline, but a DJ who
        // never dances it binds 'SLS' to Salsa. Codes are gap-filled, so the overlay's alias wins the
        // key — and writing 'SLS' for St. Louis Shag would read back as Salsa. Both files are correct;
        // the round-trip is not. The mirror must notice and fall back.
        var personal = new TaxonomyValueTranslator(
            Vocab.Customize().AddAlias(SocialDanceVocabulary.DancesDimension, "Salsa", "SLS").Build(),
            "; ", PackIds.Swing, PackIds.Blues, PackIds.StreetLatin);

        var mirrored = personal.CanonicalToMirror(FieldIds.Dance, "St. Louis Shag");

        Assert.NotEqual("SLS", mirrored);
        Assert.Equal("St. Louis Shag", personal.MirrorToCanonical(FieldIds.Dance, mirrored));

        // The rebound code still resolves to what its owner meant, and unaffected dances keep their code.
        Assert.Equal("Salsa", personal.MirrorToCanonical(FieldIds.Dance, "SLS"));
        Assert.Equal("LH", personal.CanonicalToMirror(FieldIds.Dance, "Lindy Hop"));
    }

    [Fact]
    public void Core_NormalizesAliasToCanonical()
    {
        Assert.Equal("Joyful", T.MirrorToCanonical(new FieldId("mood"), "Happy"));   // alias → canonical
        Assert.Equal("Joyful", T.CanonicalToMirror(new FieldId("mood"), "Joyful"));  // core mirror = canonical
    }

    [Fact]
    public void Unresolvable_And_UnknownField_PassThrough()
    {
        Assert.Equal("Totally Made Up", T.MirrorToCanonical(new FieldId("genre"), "Totally Made Up"));
        Assert.Equal("whatever", T.MirrorToCanonical(new FieldId("not-a-field"), "whatever"));
        Assert.Null(T.MirrorToCanonical(FieldIds.Dance, null));
    }
}
