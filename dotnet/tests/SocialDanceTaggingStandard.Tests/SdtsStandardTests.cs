using SocialDanceTaggingStandard;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>The generated standard-version constant and its compare-on-load contract (SP2 point 1):
/// every consumer stamps and compares ONE authoritative value instead of minting its own version space.</summary>
public sealed class SdtsStandardTests
{
    [Fact]
    public void Version_IsPresent() => Assert.False(string.IsNullOrWhiteSpace(SdtsStandard.Version));

    [Fact]
    public void IsCurrent_MatchesOnlyTheExactStampedVersion()
    {
        Assert.True(SdtsStandard.IsCurrent(SdtsStandard.Version));
        Assert.False(SdtsStandard.IsCurrent("0.0.0"));
        Assert.False(SdtsStandard.IsCurrent(null));                       // no stamp is not current
        Assert.False(SdtsStandard.IsCurrent(SdtsStandard.Version + " ")); // ordinal exact-match, no trimming
    }
}
