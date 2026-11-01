using SocialDanceTaggingStandard.MediaMonkey;
using SocialDanceTaggingStandard.Sync;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class MirrorMapTests
{
    private static FieldMirror F(string field, string canonical, string mirror) =>
        new(new FieldId(field), new TagKey(canonical), new TagKey(mirror));

    [Fact]
    public void Compose_MergesStandardAndPrivateFields()
    {
        var standard = new[] { F("dances", "SDTS_DANCES", "Songs-DB_Occasion") };
        var privateFields = new[] { F("sb_energy", "SB-Energy", "Songs-DB_Custom2") };

        IReadOnlyList<FieldMirror> merged = MirrorMap.Compose(standard, privateFields);

        Assert.Equal(2, merged.Count);
        Assert.Contains(merged, m => m.Field.Value == "sb_energy");
    }

    [Fact]
    public void Compose_DuplicateFieldId_Throws()
    {
        var standard = new[] { F("dances", "SDTS_DANCES", "Songs-DB_Occasion") };
        var clash = new[] { F("dances", "SDTS_OTHER", "Songs-DB_Custom2") };

        Assert.Throws<ArgumentException>(() => MirrorMap.Compose(standard, clash));
    }

    [Fact]
    public void Compose_DuplicateCanonicalKey_Throws()
    {
        var standard = new[] { F("dances", "SDTS_DANCES", "Songs-DB_Occasion") };
        var clash = new[] { F("other", "SDTS_DANCES", "Songs-DB_Custom2") };

        Assert.Throws<ArgumentException>(() => MirrorMap.Compose(standard, clash));
    }

    [Fact]
    public void Compose_DuplicateMirrorKey_Throws()
    {
        var standard = new[] { F("dances", "SDTS_DANCES", "Songs-DB_Occasion") };
        var clash = new[] { F("other", "SDTS_OTHER", "Songs-DB_Occasion") };

        Assert.Throws<ArgumentException>(() => MirrorMap.Compose(standard, clash));
    }

    [Fact]
    public void Validate_StandardMediaMonkeyMap_Passes()
    {
        // Should not throw — the shipped map must be internally consistent.
        MirrorMap.Validate(MediaMonkeyFrames.Map);
    }
}
