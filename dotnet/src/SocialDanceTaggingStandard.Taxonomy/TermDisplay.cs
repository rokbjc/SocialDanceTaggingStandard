namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// Per-term display metadata for UI surfaces (chips, dropdowns, player buttons), authored in the term's
/// <c>display:</c> block under <c>data/vocabulary/</c>. Any field may be null.
/// </summary>
public sealed class TermDisplay
{
    /// <summary>Creates term display metadata.</summary>
    public TermDisplay(string? code, string? emoji, string? icon, string? color, string? shortLabel = null)
    {
        Code = code;
        Emoji = emoji;
        Icon = icon;
        Color = color;
        ShortLabel = shortLabel;
    }

    /// <summary>Short bare abbreviation, e.g. <c>"LH"</c>, <c>"tp"</c> (never the legacy <c>#</c> form).</summary>
    public string? Code { get; }

    /// <summary>A single unicode emoji, e.g. <c>"🎺"</c>.</summary>
    public string? Emoji { get; }

    /// <summary>Iconify icon name as <c>prefix:name</c> (any set — <c>mdi:</c>, <c>game-icons:</c>, …;
    /// verified to exist via the Iconify API). Render with an Iconify component/SVG.</summary>
    public string? Icon { get; }

    /// <summary>Hex chip color, e.g. <c>"#E0A030"</c>.</summary>
    public string? Color { get; }

    /// <summary>Optional short chip caption, distinct from <see cref="Code"/> (e.g. <c>"Chi-Blues"</c> vs
    /// code <c>"ChB"</c>). Null unless authored or set by a personal-profile display override.</summary>
    public string? ShortLabel { get; }
}
