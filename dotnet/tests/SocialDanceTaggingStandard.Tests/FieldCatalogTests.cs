using SocialDanceTaggingStandard;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>The runtime field-metadata catalog (generated from data/fields.yaml): every field is
/// reachable through its <see cref="FieldId"/>, and the backing-dimension mapping matches the two
/// deliberate splits the spec documents.</summary>
public sealed class FieldCatalogTests
{
    [Fact]
    public void EveryFieldId_HasMetadata_ReachableAtRuntime()
    {
        // FieldIds.ByValue also carries the standard-library ids (title/artist/…), which are NOT SDTS
        // fields and are deliberately absent from the catalog; the taxonomy fields must all be present.
        foreach (var meta in FieldCatalog.All.Values)
        {
            Assert.False(string.IsNullOrWhiteSpace(meta.DisplayName));
            Assert.False(string.IsNullOrWhiteSpace(meta.Description));
            Assert.Same(meta, FieldCatalog.For(meta.Id));
        }
    }

    [Fact]
    public void For_KnownField_ReturnsItsDisplayNameAndDescription()
    {
        var feel = FieldCatalog.For(FieldIds.RhythmicFeel);

        Assert.NotNull(feel);
        Assert.Equal("Rhythmic Feel", feel!.DisplayName);
        Assert.Contains("feel", feel.Description, System.StringComparison.OrdinalIgnoreCase);
        Assert.Equal("taxonomy", feel.Layer);
    }

    [Fact]
    public void For_StandardLibraryFieldId_IsNull()
    {
        // A title/artist FieldId exists for keying a taxonomy-agnostic map, but it is not an SDTS field
        // and carries no spec metadata — the catalog says so by returning null rather than a blank record.
        Assert.Null(FieldCatalog.For(FieldIds.Title));
        Assert.Null(FieldCatalog.For(FieldIds.Artist));
    }

    [Theory]
    [InlineData("dance", "dances")]                 // the field is singular; its dimension is the plural union
    [InlineData("instrument_led", "instrument")]    // bound to the unified instrument dimension
    [InlineData("genre", "genre")]                  // the identity case
    public void BackingDimension_TracksTheDeliberateSplits(string fieldId, string expectedDimension)
        => Assert.Equal(expectedDimension, FieldCatalog.For(new FieldId(fieldId))!.BackingDimension);

    [Theory]
    [InlineData("bpm")]      // a number
    [InlineData("meter")]    // an intrinsic enum, not a vocabulary dimension
    [InlineData("regions")]  // a composite
    public void BackingDimension_IsNull_ForFieldsWithNoVocabulary(string fieldId)
        => Assert.Null(FieldCatalog.For(new FieldId(fieldId))!.BackingDimension);
}
