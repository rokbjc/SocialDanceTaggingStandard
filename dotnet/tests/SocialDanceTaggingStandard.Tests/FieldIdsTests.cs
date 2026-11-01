using SocialDanceTaggingStandard;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class FieldIdsTests
{
    // The standard music-library fields (data/fields.yaml `standard_fields`) are generated as FieldIds
    // and resolve via ByValue by their wire token — the taxonomy-agnostic field-map lookup a search UI
    // (e.g. a consumer's search query) keys on. Guards their presence without re-listing magic strings elsewhere.
    [Theory]
    [InlineData("title")]
    [InlineData("artist")]
    [InlineData("album")]
    [InlineData("key")]
    [InlineData("grouping")]
    [InlineData("valence")]
    [InlineData("year")]
    public void StandardLibraryField_resolves_by_token(string id)
    {
        Assert.True(FieldIds.ByValue.TryGetValue(id, out var field), $"FieldIds.ByValue missing '{id}'");
        Assert.Equal(id, field.Value);
    }

    [Fact]
    public void StandardLibraryField_members_are_exposed()
    {
        Assert.Equal("title", FieldIds.Title.Value);
        Assert.Equal("valence", FieldIds.Valence.Value);
        Assert.Equal("year", FieldIds.Year.Value);
    }
}
