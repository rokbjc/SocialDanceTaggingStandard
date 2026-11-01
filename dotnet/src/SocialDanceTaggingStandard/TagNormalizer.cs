namespace SocialDanceTaggingStandard;

/// <summary>
/// Pure helpers for normalizing semicolon-multi-value tag fields. Centralised here so
/// every writer (UI, MCP, ChatGPT analyzer, bulk migrations) lands on identical wire
/// format on disk.
/// </summary>
public static class TagNormalizer
{
    private static readonly char[] AllowedSeparators = [';', ','];

    /// <summary>
    /// Splits a multi-value field on <c>;</c> or <c>,</c> (legacy hand-edits often used
    /// comma), trims whitespace, drops empties. Slash <c>/</c> is intentionally NOT a
    /// separator — slash inside a value is meaningful (e.g. "12/8" time signature).
    /// Convert pre-existing slash-delimited fields with <see cref="NormalizeMulti"/>
    /// before passing here.
    /// </summary>
    public static string[] Split(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(AllowedSeparators,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// Joins values with the canonical <c>"; "</c> delimiter.
    /// Ignores null/empty entries and trims whitespace per value.
    /// </summary>
    public static string Join(IEnumerable<string?> values)
    {
        var clean = values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!.Trim())
            .Where(v => v.Length > 0);
        return string.Join("; ", clean);
    }

    /// <summary>
    /// Normalizes a single multi-value field's wire form. Splits on any of <c>; , /</c>
    /// (yes, slash here — this is the migration path for legacy slash-delimited fields
    /// like genre <c>"Hot Jazz / Stride"</c>), trims, dedupes case-insensitively while
    /// preserving first-seen casing, rejoins with <c>"; "</c>.
    ///
    /// Use on:
    /// - Read path when the existing tag may have legacy slash delimiters
    /// - Write path when accepting input from external sources (MCP, ChatGPT, manual entry)
    /// </summary>
    public static string NormalizeMulti(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;
        var parts = raw
            .Split([';', ',', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var keep = new List<string>(parts.Length);
        foreach (var p in parts)
        {
            if (seen.Add(p))
                keep.Add(p);
        }
        return string.Join("; ", keep);
    }

    /// <summary>
    /// Normalizes an array of values (assume already split by caller) — trims, dedupes
    /// case-insensitively keeping first-seen casing. Returns the cleaned values; caller
    /// can <see cref="Join"/> them when writing.
    /// </summary>
    public static string[] NormalizeArray(IEnumerable<string?> values)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var keep = new List<string>();
        foreach (var v in values)
        {
            if (string.IsNullOrWhiteSpace(v)) continue;
            var trimmed = v.Trim();
            if (seen.Add(trimmed)) keep.Add(trimmed);
        }
        return [.. keep];
    }
}
