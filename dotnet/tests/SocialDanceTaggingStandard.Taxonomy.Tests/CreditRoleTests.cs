using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>The SDTS-authored TIPL (Involved People List) role vocabulary: an OPEN model where a known
/// role resolves to a typed term (code/description) and an unknown one passes through carrying its raw
/// string. Mirrors what a consumer does with a raw <c>InvolvedPerson.Role</c>.</summary>
public sealed class CreditRoleTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();

    [Fact]
    public void Resolve_KnownRole_ReturnsTypedTerm()
    {
        var v = Vocab.CreditRole().Resolve("Producer");
        Assert.True(v.IsKnown);
        Assert.Equal("Producer", v.Canonical);
        Assert.Equal("PRD", v.Term.Code.Value);
    }

    [Fact]
    public void Resolve_Alias_ResolvesToCanonical()
    {
        var v = Vocab.CreditRole().Resolve("Mixed By");
        Assert.True(v.IsKnown);
        Assert.Equal("Mixing Engineer", v.Canonical);
    }

    [Fact]
    public void Resolve_UnknownRole_PassesThroughRawString()
    {
        var v = Vocab.CreditRole().Resolve("Tape Op");
        Assert.False(v.IsKnown);
        Assert.Equal("Tape Op", v.Canonical);      // the Generic term carries the raw string
        Assert.Equal("Tape Op", v.StoredValue);
    }
}
