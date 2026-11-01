using SocialDanceTaggingStandard.Id3;
using Xunit;
using Id3v2Tag = TagLib.Id3v2.Tag;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>The canonical user comment (empty-description COMM) is the user's free text: a tool adds to it,
/// it never rewrites it.</summary>
public sealed class CanonicalCommentTests
{
    [Fact]
    public void Append_ToAnEmptyComment_WritesTheNote()
    {
        var tag = new Id3v2Tag();

        tag.AppendCanonicalComment("Great clarinet solo at 1:12");

        Assert.Equal("Great clarinet solo at 1:12", tag.GetCanonicalComment());
    }

    [Fact]
    public void Append_KeepsTheUsersTextVerbatim_AndJoinsWithSemicolon()
    {
        var tag = new Id3v2Tag();
        tag.SetCanonicalComment("From the 1938 concert; live, some crowd noise");

        tag.AppendCanonicalComment("odd ending");

        Assert.Equal("From the 1938 concert; live, some crowd noise; odd ending", tag.GetCanonicalComment());
    }

    [Fact]
    public void Append_AnEntryAlreadyPresent_ChangesNothing()
    {
        var tag = new Id3v2Tag();
        tag.SetCanonicalComment("odd ending; long intro");

        tag.AppendCanonicalComment("long intro");
        tag.AppendCanonicalComment("  odd ending  ");

        Assert.Equal("odd ending; long intro", tag.GetCanonicalComment());
    }

    [Fact]
    public void Append_ABlankNote_ChangesNothing()
    {
        var tag = new Id3v2Tag();
        tag.SetCanonicalComment("keep me");

        tag.AppendCanonicalComment("   ");

        Assert.Equal("keep me", tag.GetCanonicalComment());
    }

    [Fact]
    public void Append_DoesNotReadOrWriteADescribedCommFrame()
    {
        // A COMM frame WITH a description (e.g. a tool's named column) is never the canonical comment.
        var tag = new Id3v2Tag();
        var column = new TagKey("Some Column");
        new Id3CommentSource(tag).Set(column, "Male");

        tag.AppendCanonicalComment("odd ending");

        Assert.Equal("odd ending", tag.GetCanonicalComment());
        Assert.Equal("Male", new Id3CommentSource(tag).Get(column));
    }
}
