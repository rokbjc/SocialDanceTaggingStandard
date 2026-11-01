using SocialDanceTaggingStandard.Sync;
using TagLib.Id3v2;
using Id3v2Tag = TagLib.Id3v2.Tag;

namespace SocialDanceTaggingStandard.Id3;

/// <summary>
/// An <see cref="ISdtsSource"/> over ID3v2 <c>COMM</c> (comment) frames keyed by description —
/// the home of the legacy MediaMonkey <c>Songs-DB_*</c> columns (e.g. key <c>Songs-DB_Occasion</c>).
/// <para>
/// A preferred language is matched first (MediaMonkey writes these columns under <c>"XXX"</c>). But
/// other tools — and other MediaMonkey installs — write the same description under a REAL language code
/// (<c>eng</c>, …). A description-only frame is then invisible to a strict-language match, so the sync
/// reads nothing where a value plainly exists (silent non-convergence) and a write mints a duplicate.
/// This source therefore falls back to the description across languages: it reads the sole other-language
/// frame when the preferred one is absent, and on write reuses that frame in place — normalising its
/// language to the preferred code — rather than minting a second frame. It never guesses when TWO or more
/// same-description frames exist under different languages; that stays a report-path conflict for a human.
/// </para>
/// Wraps a live <see cref="Id3v2Tag"/> so a caller that already owns an open tag (e.g. inside its
/// own race-safe save wrapper) stays in control of file lifetime.
/// </summary>
public sealed class Id3CommentSource : ISdtsSource
{
    private readonly Id3v2Tag _tag;
    private readonly string _language;

    /// <summary>Creates the source over an already-open ID3v2 tag for the given preferred COMM language
    /// (defaults to <see cref="Id3Language.Unspecified"/> — the "XXX" code MediaMonkey writes under).</summary>
    public Id3CommentSource(Id3v2Tag tag, Id3Language? language = null)
    {
        _tag = tag ?? throw new ArgumentNullException(nameof(tag));
        _language = (language ?? Id3Language.Unspecified).Value;
    }

    /// <summary>Creates the source over a shared <see cref="Id3File"/> (convenience).</summary>
    public Id3CommentSource(Id3File file, Id3Language? language = null)
        : this((file ?? throw new ArgumentNullException(nameof(file))).Tag, language)
    {
    }

    /// <inheritdoc />
    public string? Get(TagKey key)
    {
        var matches = Candidates(key);
        // Preferred language wins; failing that, a single other-language frame is the value. Two or more
        // under different languages is ambiguous — read nothing rather than guess which one is authoritative.
        CommentsFrame? frame = Preferred(matches) ?? Sole(matches);
        string? text = frame?.Text;
        return string.IsNullOrEmpty(text) ? null : text;
    }

    /// <inheritdoc />
    public void Set(TagKey key, string? value)
    {
        var matches = Candidates(key);
        CommentsFrame? preferred = Preferred(matches);

        if (string.IsNullOrEmpty(value))
        {
            // Clear exactly what Get would return: the preferred frame, or the sole other-language one.
            CommentsFrame? target = preferred ?? Sole(matches);
            if (target is not null)
            {
                _tag.RemoveFrame(target);
            }

            return;
        }

        if (preferred is not null)
        {
            preferred.Text = value;
            return;
        }

        // No preferred-language frame. Reuse the sole other-language frame in place — normalise its language
        // so a strict reader (MediaMonkey) sees it — rather than mint a second same-description frame.
        CommentsFrame? reuse = Sole(matches);
        if (reuse is not null)
        {
            reuse.Language = _language;
            reuse.Text = value;
            return;
        }

        CommentsFrame frame = CommentsFrame.Get(_tag, key.Value, _language, create: true);
        frame.Text = value;
    }

    // Every COMM frame carrying this description, whatever its language.
    private List<CommentsFrame> Candidates(TagKey key) =>
        [.. _tag.GetFrames<CommentsFrame>().Where(f => string.Equals(f.Description, key.Value, StringComparison.Ordinal))];

    private CommentsFrame? Preferred(List<CommentsFrame> matches) =>
        matches.FirstOrDefault(f => string.Equals(f.Language, _language, StringComparison.Ordinal));

    private static CommentsFrame? Sole(List<CommentsFrame> matches) =>
        matches.Count == 1 ? matches[0] : null;
}
