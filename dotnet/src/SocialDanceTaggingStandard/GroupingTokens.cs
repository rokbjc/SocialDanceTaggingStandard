namespace SocialDanceTaggingStandard;

/// <summary>
/// Tokenizer for multi-value `; `-separated tag fields (Grouping/TIT1 and the
/// MM5 COMM fields that share the convention). Splits on the canonical
/// semicolon plus the legacy comma delimiter (pre-2026 writers used commas),
/// trimming whitespace and dropping empty entries. Token comparison is
/// case-insensitive — casing in legacy files is inconsistent until a
/// migration normalizes it.
/// </summary>
public static class GroupingTokens
{
    private static readonly char[] Delimiters = [';', ','];

    /// <summary>Splits a multi-value tag string into its trimmed, non-empty tokens
    /// (on the canonical <c>;</c> or the legacy <c>,</c>). Returns an empty array for null/blank.</summary>
    public static string[] Split(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw.Split(Delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    /// <summary>True when <paramref name="raw"/> contains <paramref name="token"/> as one of its
    /// tokens (case-insensitive).</summary>
    public static bool Has(string? raw, string token)
        => Split(raw).Any(t => string.Equals(t, token, StringComparison.OrdinalIgnoreCase));
}
