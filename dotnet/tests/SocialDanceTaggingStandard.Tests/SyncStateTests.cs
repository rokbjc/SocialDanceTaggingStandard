using SocialDanceTaggingStandard.Sync;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class SyncStateTests
{
    [Fact]
    public void Parse_NullOrBlank_IsFresh()
    {
        Assert.Null(SyncState.Parse(null).BaselineHash(new FieldId("anything")));
        Assert.Null(SyncState.Parse("   ").BaselineHash(new FieldId("anything")));
    }

    [Fact]
    public void SetThenBaselineHash_RoundTrips()
    {
        var state = new SyncState();
        state.Set(new FieldId("dances"), "deadbeef");

        Assert.Equal("deadbeef", state.BaselineHash(new FieldId("dances")));
        Assert.Null(state.BaselineHash(new FieldId("genre")));
    }

    [Fact]
    public void Encode_Then_Parse_PreservesEntries()
    {
        var state = new SyncState();
        state.Set(new FieldId("dances"), "aaaa1111");
        state.Set(new FieldId("theme"), "bbbb2222");

        SyncState round = SyncState.Parse(state.Encode());

        Assert.Equal("aaaa1111", round.BaselineHash(new FieldId("dances")));
        Assert.Equal("bbbb2222", round.BaselineHash(new FieldId("theme")));
    }

    [Fact]
    public void Encode_UsesStableFieldOrder()
    {
        var a = new SyncState();
        a.Set(new FieldId("zeta"), "1");
        a.Set(new FieldId("alpha"), "2");

        var b = new SyncState();
        b.Set(new FieldId("alpha"), "2");
        b.Set(new FieldId("zeta"), "1");

        Assert.Equal(a.Encode(), b.Encode());
        Assert.StartsWith("alpha=", a.Encode());
    }

    [Fact]
    public void Encode_MatchesCompactFormat()
    {
        var state = new SyncState();
        state.Set(new FieldId("a"), "1");
        state.Set(new FieldId("b"), "2");

        Assert.Equal("a=1; b=2", state.Encode());
    }

    [Fact]
    public void Remove_ForgetsBaseline()
    {
        var state = new SyncState();
        state.Set(new FieldId("a"), "1");
        state.Remove(new FieldId("a"));

        Assert.Null(state.BaselineHash(new FieldId("a")));
    }

    [Fact]
    public void Parse_ToleratesWhitespaceAndBlankEntries()
    {
        SyncState state = SyncState.Parse("  a = 1 ;; b=2 ; ");

        Assert.Equal("1", state.BaselineHash(new FieldId("a")));
        Assert.Equal("2", state.BaselineHash(new FieldId("b")));
    }
}
