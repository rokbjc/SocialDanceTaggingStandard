using SocialDanceTaggingStandard.MediaMonkey;
using SocialDanceTaggingStandard.Sync;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class MediaMonkeyFramesTests
{
    [Fact]
    public void Map_HasNoDuplicateKeys()
    {
        IReadOnlyList<FieldMirror> map = MediaMonkeyFrames.Map;

        Assert.Equal(map.Count, map.Select(m => m.Field).Distinct().Count());
        Assert.Equal(map.Count, map.Select(m => m.CanonicalKey).Distinct().Count());
        Assert.Equal(map.Count, map.Select(m => m.MirrorKey).Distinct().Count());
    }

    [Fact]
    public void CanonicalKeys_AreSdtNamespaced()
    {
        Assert.All(MediaMonkeyFrames.Map, m => Assert.StartsWith("SDTS_", m.CanonicalKey.Value));
    }

    [Fact]
    public void MirrorKeys_AreSongsDbFrames()
    {
        Assert.All(MediaMonkeyFrames.Map, m => Assert.StartsWith("Songs-DB_", m.MirrorKey.Value));
    }

    [Fact]
    public void SyncStateKey_DoesNotCollideWithAnyMappedField()
    {
        Assert.DoesNotContain(MediaMonkeyFrames.Map, m => m.CanonicalKey.Value == MediaMonkeyFrames.SyncStateKey);
    }

    [Fact]
    public void DancesMapping_IsTheOccasionFrame()
    {
        FieldMirror dances = MediaMonkeyFrames.Map.Single(m => m.Field == FieldIds.Dance);

        Assert.Equal("SDTS_DANCES", dances.CanonicalKey.Value);
        Assert.Equal("Songs-DB_Occasion", dances.MirrorKey.Value);
    }

    [Fact]
    public void Language_IsMediaMonkeyConvention()
    {
        Assert.Equal("XXX", MediaMonkeyFrames.Language.Value);
    }

    [Fact]
    public void Dances_MirrorsToOccasionAndMusicMatchSituation()
    {
        FieldMirror dances = MediaMonkeyFrames.Map.Single(m => m.Field == FieldIds.Dance);

        Assert.Equal(["Songs-DB_Occasion", "MusicMatch_Situation"], dances.MirrorKeys.Select(k => k.Value));
    }

    [Fact]
    public void Map_StillValidatesAfterDualMirror()
    {
        MirrorMap.Validate(MediaMonkeyFrames.Map); // must not throw
    }
}
