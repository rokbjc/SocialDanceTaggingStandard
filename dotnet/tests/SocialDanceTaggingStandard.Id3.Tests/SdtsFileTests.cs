using SocialDanceTaggingStandard.Id3;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class SdtsFileTests
{
    [Fact]
    public void Container_Backed_Fields_And_Save_RoundTrip()
    {
        var container = new InMemoryContainer();
        var file = new SdtsFile(container);

        file.Genre = ["Swing", "Hot Jazz"];
        file.Bpm = 232;
        file.Rating = 196;
        file.Save();

        Assert.Equal(["Swing", "Hot Jazz"], file.Genre);      // reads back through the container
        Assert.Equal(232, file.Bpm);
        Assert.Equal(196, file.Rating);
        Assert.Equal(["Swing", "Hot Jazz"], container.Genres); // landed on the container
        Assert.True(container.Saved);
    }

    [Fact]
    public void Constructor_Rejects_Null_Container()
        => Assert.Throws<System.ArgumentNullException>(() => new SdtsFile(null!));

    [Fact]
    public void SdtsFile_Also_Exposes_Base_SDTS_Accessors_Over_The_Container_Source()
    {
        var file = new SdtsFile(new InMemoryContainer());

        file.PlaybackStart = System.TimeSpan.FromSeconds(8);   // inherited SDTS_* accessor

        Assert.Equal(System.TimeSpan.FromSeconds(8), file.PlaybackStart);
        Assert.Equal("8000", file.Container.UserText.Get(new TagKey("SDTS_PLAYBACK_START")));
    }
}
