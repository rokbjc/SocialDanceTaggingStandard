using SocialDanceTaggingStandard.Sync;
using TagLib.Id3v2;
using Id3v2Tag = TagLib.Id3v2.Tag;

namespace SocialDanceTaggingStandard.Id3;

/// <summary>
/// An <see cref="ISdtsSource"/> over ID3v2 <c>TXXX</c> (user-text) frames, keyed by frame
/// description — the home of the portable SDTS fields (e.g. key <c>SDTS_DANCES</c> →
/// <c>TXXX:SDTS_DANCES</c>). Multi-values are kept as the SDTS <c>"; "</c>-joined string.
/// Wraps a live <see cref="Id3v2Tag"/> so a caller that already owns an open tag (e.g. inside its
/// own race-safe save wrapper) stays in control of file lifetime.
/// </summary>
public sealed class Id3UserTextSource : ISdtsSource
{
    private readonly Id3v2Tag _tag;

    /// <summary>Creates the source over an already-open ID3v2 tag.</summary>
    public Id3UserTextSource(Id3v2Tag tag) =>
        _tag = tag ?? throw new ArgumentNullException(nameof(tag));

    /// <summary>Creates the source over a shared <see cref="Id3File"/> (convenience).</summary>
    public Id3UserTextSource(Id3File file)
        : this((file ?? throw new ArgumentNullException(nameof(file))).Tag)
    {
    }

    /// <inheritdoc />
    public string? Get(TagKey key)
    {
        var frame = UserTextInformationFrame.Get(_tag, key.Value, create: false);
        if (frame is null) return null;

        string text = string.Join("; ", frame.Text);
        return string.IsNullOrEmpty(text) ? null : text;
    }

    /// <inheritdoc />
    public void Set(TagKey key, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            var existing = UserTextInformationFrame.Get(_tag, key.Value, create: false);
            if (existing is not null) _tag.RemoveFrame(existing);

            return;
        }

        var frame = UserTextInformationFrame.Get(_tag, key.Value, create: true);
        frame.Text = [value];
    }
}
