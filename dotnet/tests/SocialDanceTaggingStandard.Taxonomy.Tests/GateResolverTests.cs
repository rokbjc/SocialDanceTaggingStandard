using SocialDanceTaggingStandard.Gates;
using SocialDanceTaggingStandard.Taxonomy.Profile;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class GateResolverTests
{
    [Fact]
    public void GateResolver_MapsLegacyTokens_ToFields()
    {
        var profile = new PersonalProfile(gateAliases:
        [
            new GateAlias("Genres", FieldIds.Genre.Value),      // legacy MM token → "genre"
            new GateAlias("Occasions", FieldIds.Dance.Value),  // renamed → "dances"
        ]);

        GateParse p = ConfirmationGates.Parse("Genres:2; Occasions:1", profile.GateResolver());

        Assert.True(p.AllRecognized);
        Assert.Equal(2, p.Gates.Get(FieldIds.Genre));
        Assert.Equal(1, p.Gates.Get(FieldIds.Dance));
        Assert.Empty(p.UnrecognizedTokens);
    }

    [Fact]
    public void GateResolver_UnmappedToken_FlaggedForRemediation()
    {
        var profile = new PersonalProfile(gateAliases: [new GateAlias("Genres", FieldIds.Genre.Value)]);

        GateParse p = ConfirmationGates.Parse("Genres:1; Comments:1", profile.GateResolver());

        Assert.False(p.AllRecognized);
        Assert.Equal(["Comments"], p.UnrecognizedTokens);   // fix by adding a GateAlias
        Assert.Equal(1, p.Gates.Get(FieldIds.Genre));               // the mapped one still resolved
    }

    [Fact]
    public void GateResolver_EmptyProfile_IsCanonicalOnly()
    {
        GateKeyResolver resolver = new PersonalProfile().GateResolver();

        Assert.False(ConfirmationGates.Parse("Genres:1", resolver).AllRecognized);   // legacy spelling unknown
        Assert.True(ConfirmationGates.Parse("genre:1", resolver).AllRecognized);     // canonical id resolves
    }
}
