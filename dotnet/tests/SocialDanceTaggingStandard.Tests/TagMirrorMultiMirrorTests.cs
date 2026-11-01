using SocialDanceTaggingStandard.Sync;
using SocialDanceTaggingStandard.TestKit;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class TagMirrorMultiMirrorTests
{
    private static readonly IReadOnlyList<FieldMirror> DualMap =
        [new FieldMirror(new FieldId("dances"), new TagKey("C"), [new TagKey("M1"), new TagKey("M2")])];

    private static TagMirror Build(InMemoryTagSource c, InMemoryTagSource m, InMemoryTagSource s) =>
        new(c, m, s, "state", DualMap);

    [Fact]
    public void WriteCanonical_WriteThrough_WritesAllMirrorKeys()
    {
        var c = new InMemoryTagSource();
        var m = new InMemoryTagSource();
        var s = new InMemoryTagSource();

        Build(c, m, s).WriteCanonical("dances", "Shag");

        Assert.Equal("Shag", c.Get(new TagKey("C")));
        Assert.Equal("Shag", m.Get(new TagKey("M1")));
        Assert.Equal("Shag", m.Get(new TagKey("M2")));
    }

    [Fact]
    public void GetCanonical_ReturnsCanonicalValueByFieldId()
    {
        var c = new InMemoryTagSource();
        var m = new InMemoryTagSource();
        var s = new InMemoryTagSource();
        c.Set(new TagKey("C"), "Collegiate Shag");

        Assert.Equal("Collegiate Shag", Build(c, m, s).GetCanonical("dances"));
    }

    [Fact]
    public void GetCanonical_UnmappedField_Throws()
    {
        var c = new InMemoryTagSource();
        var m = new InMemoryTagSource();
        var s = new InMemoryTagSource();

        Assert.Throws<ArgumentException>(() => Build(c, m, s).GetCanonical("nope"));
    }

    [Fact]
    public void Export_OneWay_WritesAllMirrorKeys()
    {
        var c = new InMemoryTagSource();
        var m = new InMemoryTagSource();
        var s = new InMemoryTagSource();
        c.Set(new TagKey("C"), "Balboa");

        Build(c, m, s).Export();

        Assert.Equal("Balboa", m.Get(new TagKey("M1")));
        Assert.Equal("Balboa", m.Get(new TagKey("M2")));
    }
}
