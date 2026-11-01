using SocialDanceTaggingStandard.Gates;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class ConfirmationGatesTests
{
    [Fact]
    public void Parse_NullOrBlank_IsEmptyAndAllRecognized()
    {
        GateParse p = ConfirmationGates.Parse(null);
        Assert.Same(ConfirmationGates.Empty, p.Gates);
        Assert.True(p.AllRecognized);
        Assert.Empty(p.UnrecognizedTokens);

        Assert.Same(ConfirmationGates.Empty, ConfirmationGates.Parse("   ").Gates);
    }

    [Fact]
    public void WithGate_ThenGet_RoundTrips()
    {
        ConfirmationGates g = ConfirmationGates.Empty
            .WithGate(FieldIds.Bpm, 1)
            .WithGate(FieldIds.Genre, 2, "a1b2");

        Assert.Equal(1, g.Get(FieldIds.Bpm));
        Assert.Equal(2, g.Get(FieldIds.Genre));
        Assert.Equal("a1b2", g.GetCrc(FieldIds.Genre));
        Assert.Null(g.Get(FieldIds.Mood));
        Assert.True(g.IsConfirmed(FieldIds.Bpm));
    }

    [Fact]
    public void Serialize_UsesFieldIdTokens_InOrdinalOrder()
    {
        // Inserted out of order; serialization is deterministic ordinal-by-id (bpm < genre).
        string wire = ConfirmationGates.Empty
            .WithGate(FieldIds.Genre, 2)
            .WithGate(FieldIds.Bpm, 1, "a1b2")
            .Serialize();

        Assert.Equal("bpm:1:a1b2; genre:2", wire);
    }

    [Fact]
    public void Parse_CanonicalTokens_Resolve_WithoutProfile()
    {
        GateParse p = ConfirmationGates.Parse("bpm:1; dance:3");

        Assert.True(p.AllRecognized);
        Assert.Equal(1, p.Gates.Get(FieldIds.Bpm));
        Assert.Equal(3, p.Gates.Get(FieldIds.Dance));
    }

    [Fact]
    public void Parse_UnknownToken_PreservedAndFlagged()
    {
        GateParse p = ConfirmationGates.Parse("bpm:1; Beats:2");

        Assert.False(p.AllRecognized);
        Assert.Equal(["Beats"], p.UnrecognizedTokens);
        // Preserved verbatim as an opaque field id — round-trips without loss (ordinal: 'B' < 'b').
        Assert.Equal(2, p.Gates.Get(new FieldId("Beats")));
        Assert.Equal("Beats:2; bpm:1", p.Gates.Serialize());
    }

    [Fact]
    public void Parse_WithAliasResolver_ResolvesLegacyToken()
    {
        GateKeyResolver aliases = token => token == "Genres" ? FieldIds.Genre : null;

        GateParse p = ConfirmationGates.Parse("Genres:1", aliases);

        Assert.True(p.AllRecognized);
        Assert.Equal(1, p.Gates.Get(FieldIds.Genre));
        Assert.Empty(p.UnrecognizedTokens);
    }

    [Fact]
    public void Parse_Then_Serialize_RoundTrips()
    {
        const string wire = "bpm:1:a1b2; genre:2; mood:3";
        Assert.Equal(wire, ConfirmationGates.Parse(wire).Gates.Serialize());
    }

    [Fact]
    public void WithGate_FieldIdWithReservedDelimiter_Throws()
    {
        Assert.Throws<ArgumentException>(() => ConfirmationGates.Empty.WithGate(new FieldId("a:b"), 1));
        Assert.Throws<ArgumentException>(() => ConfirmationGates.Empty.WithGate(new FieldId("a;b"), 1));
    }

    [Fact]
    public void Parse_MalformedCrc_DroppedGateStaysIntact()
    {
        GateParse p = ConfirmationGates.Parse("bpm:1:zzzz");   // 4 chars but not hex

        Assert.Equal(1, p.Gates.Get(FieldIds.Bpm));
        Assert.Null(p.Gates.GetCrc(FieldIds.Bpm));
    }
}
