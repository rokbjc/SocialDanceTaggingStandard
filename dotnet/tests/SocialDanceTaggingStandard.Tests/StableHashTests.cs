using SocialDanceTaggingStandard.Internal;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class StableHashTests
{
    // Normative vectors — must match docs/standard/stable-hash.md and conformance/vectors/hash.json.
    [Theory]
    [InlineData(null, "00000000")]
    [InlineData("", "00000000")]
    [InlineData("Lindy Hop", "41379818")]
    [InlineData("dances", "784f703d")]
    [InlineData("Lindy Hop; Balboa", "9e33e3a8")]
    [InlineData("Café", "cfc290a9")] // "Café" — exercises multi-byte UTF-8 (escape = encoding-safe source)
    [InlineData("120", "733864ae")]
    [InlineData("original", "a92b8b72")]
    public void Of_MatchesNormativeVectors(string? input, string expected) =>
        Assert.Equal(expected, StableHash.Of(input));

    [Fact]
    public void Of_IsDeterministic() =>
        Assert.Equal(StableHash.Of("Shim Sham"), StableHash.Of("Shim Sham"));

    [Fact]
    public void Of_NullAndEmpty_ShareTheSentinel() =>
        Assert.Equal(StableHash.Of(null), StableHash.Of(string.Empty));

    [Fact]
    public void Of_AlwaysEightLowercaseHex()
    {
        string h = StableHash.Of("anything");
        Assert.Equal(8, h.Length);
        Assert.Matches("^[0-9a-f]{8}$", h);
    }
}
