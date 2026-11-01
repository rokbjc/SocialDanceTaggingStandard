using SocialDanceTaggingStandard.Id3;
using TagLib.Id3v2;
using Xunit;
using Id3v2Tag = TagLib.Id3v2.Tag;

namespace SocialDanceTaggingStandard.Tests;

public sealed class Id3TagSourceTests
{
    private static List<CommentsFrame> Occasion(Id3v2Tag tag) =>
        [.. tag.GetFrames<CommentsFrame>().Where(f => f.Description == "Songs-DB_Occasion")];
    [Fact]
    public void UserTextSource_OverLiveTag_RoundTrips()
    {
        var tag = new Id3v2Tag();
        var src = new Id3UserTextSource(tag);

        src.Set(new TagKey("SDTS_RHYTHMIC_FEEL"), "Shuffle");

        Assert.Equal("Shuffle", src.Get(new TagKey("SDTS_RHYTHMIC_FEEL")));
        Assert.Equal("Shuffle", new Id3UserTextSource(tag).Get(new TagKey("SDTS_RHYTHMIC_FEEL"))); // value lives on the tag
    }

    [Fact]
    public void UserTextSource_SetEmpty_RemovesFrame()
    {
        var tag = new Id3v2Tag();
        var src = new Id3UserTextSource(tag);
        src.Set(new TagKey("SDTS_RHYTHMIC_FEEL"), "Shuffle");

        src.Set(new TagKey("SDTS_RHYTHMIC_FEEL"), null);

        Assert.Null(src.Get(new TagKey("SDTS_RHYTHMIC_FEEL")));
    }

    [Fact]
    public void CommentSource_OverLiveTag_RoundTrips()
    {
        var tag = new Id3v2Tag();
        var src = new Id3CommentSource(tag, Id3Language.Unspecified);

        src.Set(new TagKey("Songs-DB_Occasion"), "#LH; #BLB");

        Assert.Equal("#LH; #BLB", src.Get(new TagKey("Songs-DB_Occasion")));
    }

    [Fact]
    public void CommentSource_ReadsForeignLanguageFrame_WhenPreferredAbsent()
    {
        // MediaMonkey writes Songs-DB_* under "XXX", but another tool (or another install) can write the same
        // description under a real language code. The value plainly exists, so the source must see it.
        var tag = new Id3v2Tag();
        tag.AddFrame(new CommentsFrame("Songs-DB_Occasion", "eng") { Text = "#LH" });

        Assert.Equal("#LH", new Id3CommentSource(tag, Id3Language.Unspecified).Get(new TagKey("Songs-DB_Occasion")));
    }

    [Fact]
    public void CommentSource_TwoLanguages_ReadsNothing_NeverGuesses()
    {
        var tag = new Id3v2Tag();
        tag.AddFrame(new CommentsFrame("Songs-DB_Occasion", "eng") { Text = "#LH" });
        tag.AddFrame(new CommentsFrame("Songs-DB_Occasion", "deu") { Text = "#BAL" });

        // Two same-description frames under different languages is ambiguous — that stays a report-path
        // conflict for a human, not a coin-flip.
        Assert.Null(new Id3CommentSource(tag, Id3Language.Unspecified).Get(new TagKey("Songs-DB_Occasion")));
    }

    [Fact]
    public void CommentSource_Write_ReusesSoleForeignFrameInPlace_NoDuplicate()
    {
        var tag = new Id3v2Tag();
        tag.AddFrame(new CommentsFrame("Songs-DB_Occasion", "eng") { Text = "#LH" });
        var src = new Id3CommentSource(tag, Id3Language.Unspecified);

        src.Set(new TagKey("Songs-DB_Occasion"), "#BAL");

        var frames = Occasion(tag);
        Assert.Single(frames);                                          // reused in place, not minted alongside
        Assert.Equal("#BAL", frames[0].Text);
        Assert.Equal(Id3Language.Unspecified.Value, frames[0].Language); // language normalised so a strict reader sees it
    }

    [Fact]
    public void CommentSource_SetNull_ClearsTheSoleForeignFrame()
    {
        var tag = new Id3v2Tag();
        tag.AddFrame(new CommentsFrame("Songs-DB_Occasion", "eng") { Text = "#LH" });
        var src = new Id3CommentSource(tag, Id3Language.Unspecified);

        src.Set(new TagKey("Songs-DB_Occasion"), null);

        Assert.Null(src.Get(new TagKey("Songs-DB_Occasion")));
        Assert.Empty(Occasion(tag));
    }

    [Fact]
    public void NullTag_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Id3UserTextSource((Id3v2Tag)null!));
    }
}
