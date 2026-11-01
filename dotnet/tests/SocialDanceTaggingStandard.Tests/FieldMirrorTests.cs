using SocialDanceTaggingStandard.Sync;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class FieldMirrorTests
{
    [Fact]
    public void SingleStringCtor_ExposesOneMirrorKey()
    {
        var m = new FieldMirror(new FieldId("dances"), new TagKey("SDTS_DANCES"), new TagKey("Songs-DB_Occasion"));

        Assert.Equal([new TagKey("Songs-DB_Occasion")], m.MirrorKeys);
        Assert.Equal(new TagKey("Songs-DB_Occasion"), m.MirrorKey); // convenience = first
    }

    [Fact]
    public void ListCtor_KeepsAllMirrorKeysInOrder()
    {
        var m = new FieldMirror(new FieldId("dances"), new TagKey("SDTS_DANCES"), [new TagKey("Songs-DB_Occasion"), new TagKey("MusicMatch_Situation")]);

        Assert.Equal([new TagKey("Songs-DB_Occasion"), new TagKey("MusicMatch_Situation")], m.MirrorKeys);
        Assert.Equal(new TagKey("Songs-DB_Occasion"), m.MirrorKey); // primary is first
    }

    [Fact]
    public void ListCtor_EmptyMirrorKeys_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new FieldMirror(new FieldId("f"), new TagKey("SDTS_F"), []));
    }

    [Fact]
    public void Validate_DuplicateAcrossEntriesSecondaryKey_Throws()
    {
        var map = new[]
        {
            new FieldMirror(new FieldId("dances"), new TagKey("SDTS_DANCES"), [new TagKey("Songs-DB_Occasion"), new TagKey("MusicMatch_Situation")]),
            new FieldMirror(new FieldId("other"), new TagKey("SDTS_OTHER"), new TagKey("MusicMatch_Situation")), // collides with dances' secondary
        };

        Assert.Throws<ArgumentException>(() => MirrorMap.Validate(map));
    }

    [Fact]
    public void Validate_DuplicateWithinOneEntry_Throws()
    {
        var map = new[]
        {
            new FieldMirror(new FieldId("dances"), new TagKey("SDTS_DANCES"), [new TagKey("Songs-DB_Occasion"), new TagKey("Songs-DB_Occasion")]),
        };

        Assert.Throws<ArgumentException>(() => MirrorMap.Validate(map));
    }
}
