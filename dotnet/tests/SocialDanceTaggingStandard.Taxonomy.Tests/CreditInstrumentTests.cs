using System.Linq;
using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>The unified instrument vocabulary (the <c>instrument_led</c> dimension) used as a TMCL credit
/// resolver: an OPEN model. Known instruments resolve to typed terms with the specific→family hierarchy;
/// credit-only instruments carry <c>lead_capable: false</c>; unknowns pass through raw. Mirrors what a
/// consumer does with a raw <c>MusicianCredit.Instrument</c>.</summary>
public sealed class CreditInstrumentTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();

    [Fact]
    public void Resolve_KnownLeadInstrument_HasHierarchyAndIsLeadCapable()
    {
        var v = Vocab.Instrument().Resolve("Guitar");
        Assert.True(v.IsKnown);
        Assert.Equal("Guitar", v.Canonical);
        Assert.Equal("string", v.Term.Parent);   // specific → family hierarchy (guitar → string)
        Assert.True(v.Term.IsLeadCapable());      // absent flag ⇒ lead-capable
    }

    [Fact]
    public void Resolve_CreditOnlyInstrument_IsKnownButNotLeadCapable()
    {
        var v = Vocab.Instrument().Resolve("Tuba");
        Assert.True(v.IsKnown);
        Assert.False(v.Term.LeadCapable ?? true); // explicitly lead_capable: false
        Assert.False(v.Term.IsLeadCapable());
    }

    [Fact]
    public void Resolve_CreditOnlyAlias_ResolvesToCanonical()
    {
        var v = Vocab.Instrument().Resolve("Sousaphone");
        Assert.True(v.IsKnown);
        Assert.Equal("Tuba", v.Canonical);
    }

    [Fact]
    public void Resolve_UnknownInstrument_PassesThroughRawString()
    {
        var v = Vocab.Instrument().Resolve("Kazoo");
        Assert.False(v.IsKnown);
        Assert.Equal("Kazoo", v.Canonical);       // the Generic term carries the raw string
    }

    [Fact]
    public void LeadSubset_ExcludesCreditOnlyInstruments()
    {
        var all = Instruments.All;
        Assert.Contains(all, i => i.Canonical == "Tuba");         // present in the unified vocabulary
        var lead = all.Where(i => i.IsLeadCapable()).ToList();
        Assert.DoesNotContain(lead, i => i.Canonical == "Tuba");  // but excluded from the lead subset
        Assert.Contains(lead, i => i.Canonical == "Guitar");
    }
}
