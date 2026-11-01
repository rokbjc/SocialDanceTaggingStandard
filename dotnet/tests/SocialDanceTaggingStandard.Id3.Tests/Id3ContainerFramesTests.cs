using SocialDanceTaggingStandard.Id3;
using Xunit;
using Id3v2Tag = TagLib.Id3v2.Tag;

namespace SocialDanceTaggingStandard.Tests;

public sealed class Id3ContainerFramesTests
{
    [Fact]
    public void Bpm_RoundTrips_ViaTbpm()
    {
        var tag = new Id3v2Tag();
        tag.SetBpm(250);
        Assert.Equal(250, tag.GetBpm());
        Assert.Equal((uint)250, tag.BeatsPerMinute);

        tag.SetBpm(null);
        Assert.Null(tag.GetBpm());
    }

    [Fact]
    public void Rating_RoundTrips_ViaPopmByte()
    {
        var tag = new Id3v2Tag();
        Assert.Null(tag.GetRating());

        tag.SetRating(196);
        Assert.Equal(196, tag.GetRating());

        tag.SetRating(null);
        Assert.Null(tag.GetRating());
    }

    [Fact]
    public void MusicianCredits_RoundTrip_AsAlternatingPairs()
    {
        var tag = new Id3v2Tag();
        tag.SetMusicianCredits([new("Piano", "Count Basie"), new("Tenor Sax", "Lester Young")]);

        var read = tag.GetMusicianCredits();
        Assert.Equal(2, read.Count);
        Assert.Equal(new MusicianCredit("Piano", "Count Basie"), read[0]);
        Assert.Equal(new MusicianCredit("Tenor Sax", "Lester Young"), read[1]);
    }

    [Fact]
    public void InvolvedPeople_RoundTrip_AsAlternatingPairs()
    {
        var tag = new Id3v2Tag();
        tag.SetInvolvedPeople([new("producer", "John Hammond"), new("arranger", "Eddie Durham")]);

        var read = tag.GetInvolvedPeople();
        Assert.Equal(2, read.Count);
        Assert.Equal(new InvolvedPerson("producer", "John Hammond"), read[0]);
    }

    [Fact]
    public void InvolvedPeople_SurviveRenderReparse_UnderV24()
    {
        var tag = new Id3v2Tag { Version = 4 };
        tag.SetInvolvedPeople([new("producer", "John Hammond")]);

        var reparsed = new Id3v2Tag(tag.Render());   // round-trip through bytes

        var read = reparsed.GetInvolvedPeople();
        Assert.Single(read);
        Assert.Equal(new InvolvedPerson("producer", "John Hammond"), read[0]);
    }
}
