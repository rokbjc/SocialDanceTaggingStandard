namespace SocialDanceTaggingStandard.Id3;

// Strongly-typed ID3 identifier kinds (CLAUDE.md coding guideline 1). These wrap the strings TagLib# wants,
// so frame ids, TXXX descriptions, COMM descriptions and language codes can never be crossed at a call
// site. They name only PORTABLE ID3 concepts (guideline 3): the MediaMonkey "Songs-DB_*"/"MusicMatch_*"
// COMM descriptions and "TXXX:Quality" are defined in SocialDanceTaggingStandard.MediaMonkey, never here.

/// <summary>
/// An ID3v2 frame id — the 4-character frame TYPE name passed to TagLib# <c>Get(...)</c> (e.g. <c>TCON</c>,
/// <c>TMOO</c>, <c>COMM</c>, <c>POPM</c>, <c>TBPM</c>, <c>TXXX</c>). A frame id is the container slot type,
/// distinct from a frame DESCRIPTION (<see cref="Id3UserTextDescription"/> / <see cref="Id3CommentDescription"/>).
/// </summary>
/// <param name="Value">The 4-char ID3v2 frame id.</param>
public readonly record struct Id3FrameId(string Value)
{
    /// <summary>The raw 4-char frame id string.</summary>
    public override string ToString() => Value;
}

/// <summary>
/// The description key of a TXXX (user-text) frame — the home of the portable SDTS surface
/// (<c>TXXX:SDTS_*</c>, e.g. <c>SDTS_DANCES</c>, <c>SDTS_MOOD</c>). Separate from
/// <see cref="Id3CommentDescription"/> so a TXXX description can never be handed to a COMM lookup.
/// </summary>
/// <param name="Value">The TXXX frame description (without the <c>TXXX:</c> prefix).</param>
public readonly record struct Id3UserTextDescription(string Value)
{
    /// <summary>The raw TXXX description string.</summary>
    public override string ToString() => Value;
}

/// <summary>
/// The description key of a COMM (comment) frame. The empty-description slot (<see cref="Canonical"/>) is the
/// portable canonical user comment (TagLib's default <c>tag.Comment</c>). MediaMonkey's custom COMM column
/// descriptions (<c>Songs-DB_*</c>, <c>MusicMatch_Situation</c>) are MM5-specific and are defined as constants
/// in <c>SocialDanceTaggingStandard.MediaMonkey</c>, not here — keeping this type portable per guideline 3.
/// </summary>
/// <param name="Value">The COMM frame description (empty for the canonical comment).</param>
public readonly record struct Id3CommentDescription(string Value)
{
    /// <summary>The raw COMM description string (empty for the canonical comment slot).</summary>
    public override string ToString() => Value;

    /// <summary>The canonical user-comment slot: the COMM frame with an empty description.</summary>
    public static readonly Id3CommentDescription Canonical = new("");
}

/// <summary>
/// An ID3 language code — the 3-byte ISO-639-2 code (or <see cref="Unspecified"/>) that, together with the
/// description, disambiguates COMM frames. Typed so the language byte is never confused with a frame id.
/// </summary>
/// <param name="Value">The 3-char language code.</param>
public readonly record struct Id3Language(string Value)
{
    /// <summary>The raw 3-char language code string.</summary>
    public override string ToString() => Value;

    /// <summary>The <c>XXX</c> "unspecified" language used for non-linguistic COMM frames.</summary>
    public static readonly Id3Language Unspecified = new("XXX");
}

/// <summary>
/// A POPM frame owner identifier — the email/owner key that selects which application's popularimeter
/// (rating) byte to read or write (e.g. <c>no@email</c>). Typed so it cannot collide with a frame id or a
/// TXXX description.
/// </summary>
/// <param name="Value">The POPM owner/email identifier.</param>
public readonly record struct PopmOwner(string Value)
{
    /// <summary>The raw POPM owner string.</summary>
    public override string ToString() => Value;

    /// <summary>The reference implementation's POPM owner (<c>no@email</c>) — the MM5/JRiver-compatible slot.</summary>
    public static readonly PopmOwner Default = new("no@email");
}
