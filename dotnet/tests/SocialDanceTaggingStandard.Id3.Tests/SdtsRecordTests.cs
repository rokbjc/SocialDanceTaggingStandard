using SocialDanceTaggingStandard.Id3;
using SocialDanceTaggingStandard.TestKit;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class SdtsRecordTests
{
    [Fact]
    public void Multi_And_Ms_And_Enum_Fields_RoundTrip_InMemory()
    {
        var source = new InMemoryTagSource();
        var record = new SdtsRecord(source);

        record.Dances = ["Lindy Hop", "Balboa"];
        record.PlaybackStart = System.TimeSpan.FromSeconds(8);
        record.EndingType = EndingType.Definitive;
        record.Energy = 7;

        Assert.Equal(["Lindy Hop", "Balboa"], record.Dances);
        Assert.Equal("Lindy Hop; Balboa", source.Get(new TagKey("SDTS_DANCES")));
        Assert.Equal(System.TimeSpan.FromSeconds(8), record.PlaybackStart);
        Assert.Equal("8000", source.Get(new TagKey("SDTS_PLAYBACK_START")));
        Assert.Equal(EndingType.Definitive, record.EndingType);
        Assert.Equal("definitive", source.Get(new TagKey("SDTS_ENDING_TYPE")));
    }

    [Fact]
    public void Gates_RoundTrip_ByFieldId_IncludingOpaqueExtension()
    {
        var record = new SdtsRecord(new InMemoryTagSource());
        record.Gates = record.Gates.WithGate(FieldIds.Bpm, 1).WithGate(new FieldId("instruments"), 1);
        Assert.True(record.Gates.IsConfirmed(FieldIds.Bpm));
        Assert.True(record.Gates.IsConfirmed(new FieldId("instruments")));   // opaque extension gate
    }

    [Fact]
    public void Regions_RoundTrip_And_SkipMalformed_InMemory()
    {
        var source = new InMemoryTagSource();
        var record = new SdtsRecord(source);

        Assert.Empty(record.Regions);   // absent frame → empty (back-compat)

        Region[] regions =
        [
            new Region(0, 3200, RegionKind.Silence),
            new Region(3200, 8500, RegionKind.Speech),
            new Region(185000, 190000, RegionKind.MusicBoundary),
        ];
        record.Regions = regions;

        Assert.Equal("0:3200:silence; 3200:8500:speech; 185000:190000:music-boundary",
            source.Get(new TagKey("SDTS_REGIONS")));
        Assert.Equal(regions, record.Regions);   // write → read round-trips by value

        // malformed (too few parts) and unknown-kind tuples are skipped on read (robust)
        source.Set(new TagKey("SDTS_REGIONS"), "0:3200:silence; garbage; 5:9:nope; 100:200:speech");
        Region[] afterSkip = [new Region(0, 3200, RegionKind.Silence), new Region(100, 200, RegionKind.Speech)];
        Assert.Equal(afterSkip, record.Regions);
    }
}
