using TagLib;
using TagLib.Id3v2;
using Tag = TagLib.Id3v2.Tag;

namespace SocialDanceTaggingStandard.Id3;

/// <summary>
/// Portable ID3v2 tag accessors for the SDTS standard-frame surface, as extension members on the TagLib#
/// <c>Tag</c>: genre (TCON), mood (TMOO), and the canonical user comment (the empty-description COMM slot).
/// MediaMonkey-specific columns (<c>COMM:Songs-DB_*</c>) live in <c>SocialDanceTaggingStandard.MediaMonkey</c>
/// (the generated mirror map) — this layer carries no MM5 knowledge.
/// </summary>
public static class Id3TaxonomyExtensions
{
    private const string CanonicalCommentSeparator = "; ";

    extension(Tag tag)
    {
        /// <summary>
        /// Reads the canonical user-comment COMM frame (description = "" or null — TagLib treats both as the
        /// "no description" canonical slot). Use this instead of <see cref="TagLib.Tag.Comment"/> when the file
        /// might carry custom COMM frames, because TagLib's default getter falls back to "first available COMM
        /// frame" and can surface a custom frame's text. Returns empty string when no canonical frame exists.
        /// </summary>
        public string GetCanonicalComment()
        {
            foreach (var f in tag.GetFrames<CommentsFrame>(Id3Frames.COMM.Value))
            {
                if (!string.IsNullOrEmpty(f.Description)) continue;
                return f.Text ?? "";
            }

            return "";
        }

        /// <summary>
        /// Writes the canonical user-comment COMM frame (empty description, language "XXX"). Drops any
        /// pre-existing empty-description COMM frames in other languages first, so a single canonical frame
        /// remains (TagLib's <c>Get</c> matches by (description, language) and would otherwise add a twin that
        /// readers pick non-deterministically). Empty <paramref name="value"/> creates an empty canonical frame
        /// so a deliberate clear persists.
        /// </summary>
        public void SetCanonicalComment(string value)
        {
            var stale = tag.GetFrames<CommentsFrame>(Id3Frames.COMM.Value)
                .Where(f => string.IsNullOrEmpty(f.Description))
                .ToList();
            foreach (var f in stale)
            {
                tag.RemoveFrame(f);
            }

            var frame = CommentsFrame.Get(tag,
                Id3CommentDescription.Canonical.Value,
                Id3Language.Unspecified.Value,
                true);
            frame.Text = value ?? "";
        }

        /// <summary>
        /// Adds a note to the canonical user comment without rewriting what is there — the SDTS convention for
        /// any tool that writes into the user's comment. An empty comment becomes the note; otherwise the note is
        /// appended after <c>"; "</c>. The existing text is kept verbatim (never reordered, trimmed or
        /// de-duplicated): it is the user's prose and may itself contain <c>"; "</c>. A blank note, or one already
        /// present as a whole <c>"; "</c>-separated entry, changes nothing, so a writer that runs twice does not
        /// stack the same note. Writes through <see cref="SetCanonicalComment"/>.
        /// </summary>
        public void AppendCanonicalComment(string note)
        {
            ArgumentNullException.ThrowIfNull(note);
            var entry = note.Trim();
            if (entry.Length == 0)
            {
                return;
            }

            var existing = tag.GetCanonicalComment();
            if (existing.Split(CanonicalCommentSeparator).Contains(entry, StringComparer.Ordinal))
            {
                return;
            }

            tag.SetCanonicalComment(existing.Length == 0 ? entry : existing + CanonicalCommentSeparator + entry);
        }

        /// <summary>The genre values (the TCON frame, via TagLib's <c>Genres</c>).</summary>
        public IEnumerable<string> GetGenres() => tag.Genres;

        /// <summary>Writes <paramref name="genres"/> to TCON as plain UTF-8 text (de-duplicated), bypassing TagLib#'s ID3v1 numeric-genre conversion.</summary>
        public void SetGenres(params IEnumerable<string> genres)
        {
            // Write TCON frame directly as plain text to avoid TagLib#'s genre-name → ID3v1 numeric code
            // conversion (which causes MediaMonkey to display raw numbers like "78" instead of "Rock & Roll").
            var frame = TextInformationFrame.Get(tag, Id3Frames.TCON.Value, StringType.UTF8, true);
            frame.Text = [.. genres.Distinct()];
        }

        /// <summary>The mood values from the TMOO frame.</summary>
        public IEnumerable<string> GetMoods()
        {
            var frame = TextInformationFrame.Get(tag, Id3Frames.TMOO.Value, StringType.UTF8, false);
            if (frame?.Text is null) yield break;

            foreach (var mood in frame.Text)
            {
                yield return mood.Trim();
            }
        }

        /// <summary>Writes <paramref name="moods"/> to the TMOO frame (no-op when unchanged).</summary>
        public void SetMoods(params IEnumerable<string> moods)
        {
            var theFrame = TextInformationFrame.Get(tag, Id3Frames.TMOO.Value, StringType.UTF8, true);

            var oldText = string.Join("; ", theFrame.Text ?? []);

            var moodArray = moods as string[] ?? [.. moods];
            var newText = string.Join("; ", moodArray);

            if (oldText != newText)
            {
                theFrame.Text = [.. moodArray];
            }
        }

        /// <summary>BPM from the standard TBPM frame; null when unset (0). Bypasses no numeric-genre quirk —
        /// TBPM is a plain integer frame.</summary>
        public int? GetBpm()
            => tag.BeatsPerMinute == 0 ? null : (int)tag.BeatsPerMinute;

        /// <summary>Writes BPM to TBPM (0/negative/null clears it).</summary>
        public void SetBpm(int? value)
            => tag.BeatsPerMinute = value is > 0 ? (uint)value.Value : 0u;

        /// <summary>The raw 0–255 POPM byte for owner <paramref name="owner"/> (default <c>no@email</c>);
        /// null when no such POPM frame exists.</summary>
        public int? GetRating(PopmOwner? owner = null)
        {
            var frame = PopularimeterFrame.Get(tag, (owner ?? PopmOwner.Default).Value, create: false);
            return frame?.Rating;
        }

        /// <summary>Writes the raw 0–255 POPM byte for <paramref name="owner"/>; null removes the frame.</summary>
        public void SetRating(int? value, PopmOwner? owner = null)
        {
            var user = (owner ?? PopmOwner.Default).Value;
            if (value is null)
            {
                var existing = PopularimeterFrame.Get(tag, user, create: false);
                if (existing is not null) tag.RemoveFrame(existing);
                return;
            }

            var frame = PopularimeterFrame.Get(tag, user, create: true);
            frame.Rating = (byte)Math.Clamp(value.Value, 0, 255);
        }

        /// <summary>Involved people (TIPL): raw (role, name) pairs from the frame's alternating string list.</summary>
        public IReadOnlyList<InvolvedPerson> GetInvolvedPeople()
            => [.. ReadPairs(tag, Id3Frames.TIPL.Value).Select(p => new InvolvedPerson(p.Key, p.Value))];

        /// <summary>Writes involved people to TIPL as an alternating (role, name) string list.</summary>
        public void SetInvolvedPeople(IReadOnlyList<InvolvedPerson> people)
            => WritePairs(tag, Id3Frames.TIPL.Value, people.Select(p => (p.Role, p.Name)));

        /// <summary>Musician credits (TMCL): raw (instrument, name) pairs.</summary>
        public IReadOnlyList<MusicianCredit> GetMusicianCredits()
            => [.. ReadPairs(tag, Id3Frames.TMCL.Value).Select(p => new MusicianCredit(p.Key, p.Value))];

        /// <summary>Writes musician credits to TMCL as an alternating (instrument, name) string list.</summary>
        public void SetMusicianCredits(IReadOnlyList<MusicianCredit> credits)
            => WritePairs(tag, Id3Frames.TMCL.Value, credits.Select(c => (c.Instrument, c.Name)));
    }

    // Alternating "paired text" frames (ID3v2.4 §4.2.2): [label0, name0, label1, name1, ...].
    // TagLib 2.3.0 has no typed TIPL/TMCL class — they are generic TextInformationFrame by 4-char id.
    private static IReadOnlyList<KeyValuePair<string, string>> ReadPairs(Tag tag, string frameId)
    {
        var frame = TextInformationFrame.Get(tag, frameId, StringType.UTF8, false);
        string[] text = frame?.Text ?? [];
        var pairs = new List<KeyValuePair<string, string>>(text.Length / 2);
        for (int i = 0; i + 1 < text.Length; i += 2)
            pairs.Add(new(text[i], text[i + 1]));
        return pairs;
    }

    private static void WritePairs(Tag tag, string frameId, IEnumerable<(string Label, string Name)> pairs)
    {
        var flat = new List<string>();
        foreach (var (label, name) in pairs)
        {
            flat.Add(label);
            flat.Add(name);
        }

        if (flat.Count == 0)
        {
            tag.RemoveFrames(frameId); // ByteVector implicit from the 4-char string
            return;
        }

        var frame = TextInformationFrame.Get(tag, frameId, StringType.UTF8, true);
        frame.Text = [.. flat];
    }
}
