using System.Text.RegularExpressions;

namespace SocialDanceTaggingStandard.Taxonomy.Ingest;

/// <summary>
/// The PARSE stage of path ingest: turns a relative path string into a captured field dictionary plus a
/// list of unrecognised tail tokens. Pure — no file I/O, no vocabulary mutation, no sync object. The MAP
/// stage (token → SDTS value) is the existing <see cref="IngestMapper"/>. See
/// <c>docs/superpowers/specs/2026-06-25-path-filename-ingest-design.md</c>.
/// </summary>
public static partial class PathParser
{
    /// <summary>True when <paramref name="relativePath"/> matches the rule's path glob
    /// (<c>*</c> = any run except the directory separator, <c>**</c> = any run including separators).</summary>
    public static bool Matches(PathRule rule, string relativePath)
    {
        string normalized = relativePath.Replace('\\', '/');
        string regex = "^" + Regex.Escape(rule.Match.Replace('\\', '/'))
            .Replace(@"\*\*", "::DOUBLESTAR::")
            .Replace(@"\*", "[^/]*")
            .Replace("::DOUBLESTAR::", ".*") + "$";
        return Regex.IsMatch(normalized, regex, RegexOptions.IgnoreCase);
    }

    /// <summary>Parse a relative path against a rule. Returns null if the glob does not match.</summary>
    public static PathCapture? Parse(string relativePath, PathRule rule)
    {
        if (!Matches(rule, relativePath)) return null;

        var fields = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        var unmapped = new List<string>();

        foreach (KeyValuePair<string, string> kv in rule.Set)
        {
            Set(kv.Key, kv.Value);
        }

        string baseName = Path.GetFileNameWithoutExtension(relativePath.Replace('\\', '/'));

        // 1. trailing +-run → rating
        if (rule.Rating is { } r)
        {
            Match m = TrailingPlus().Match(baseName);
            if (m.Success)
            {
                baseName = baseName[..m.Index].TrimEnd();
                int count = m.Value.TrimEnd().Length;
                int scaled = r.Scale == RatingScale.Clamp10
                    ? Math.Clamp(count, 1, 10)
                    : (int)Math.Round(1 + (9.0 * (count - 1) / Math.Max(1, r.Max - 1)));
                Set(r.To, scaled.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }

        // 2. year (anywhere) → recording_year; remove the matched token
        if (rule.Year is { } y)
        {
            Match ym = Regex.Match(baseName, y.Pattern);
            if (ym.Success)
            {
                Set(y.To, ym.Value);
                baseName = (baseName[..ym.Index] + baseName[(ym.Index + ym.Length)..]).Replace("()", "").Trim();
            }
        }

        // 3. artist / title split on the first " - " (double-space tolerant)
        Match sep = Regex.Match(baseName, @"\s+-\s+");
        string? artist = sep.Success ? baseName[..sep.Index].Trim() : null;
        string rest = sep.Success ? baseName[(sep.Index + sep.Length)..].Trim() : baseName.Trim();
        if (artist is not null) Set("artist", artist);

        // 4. peel a maximal trailing run of RECOGNISED tail tokens (right-to-left)
        var codes = new HashSet<string>(rule.Codes.Select(c => c.Token), StringComparer.OrdinalIgnoreCase);
        var tokens = rest.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        var tail = new List<string>();
        while (tokens.Count > 0 && codes.Contains(tokens[^1]))
        {
            tail.Insert(0, tokens[^1]);
            tokens.RemoveAt(tokens.Count - 1);
        }

        foreach (string t in tail) Set("path", t);

        // 5. when the dictionary is empty/partial, surface trailing code-like tokens as audit candidates
        //    (never assigned to a dimension — flag, don't guess)
        if (rule.OnUnknownToken == UnknownTokenPolicy.Flag)
        {
            foreach (string t in tokens.Where(LooksLikeCode))
            {
                unmapped.Add(t);
            }
        }

        string title = string.Join(' ', tokens).Trim();
        if (title.Length > 0) Set("title", title);

        return new PathCapture(
            fields.ToDictionary(kv => kv.Key, IReadOnlyList<string> (kv) => kv.Value, StringComparer.OrdinalIgnoreCase),
            unmapped);

        void Set(string key, string value)
        {
            if (!fields.TryGetValue(key, out List<string>? list)) fields[key] = list = [];

            if (!string.IsNullOrWhiteSpace(value)) list.Add(value);
        }
    }

    /// <summary>A heuristic for "this trailing token looks like a code worth auditing" — governs ONLY the
    /// <see cref="PathCapture.Unmapped"/> list, never a dimension assignment.</summary>
    private static bool LooksLikeCode(string t)
        => t.Length <= 5 && (t.All(char.IsUpper) || t.Any(char.IsDigit) || (t.All(char.IsLower) && t.Length <= 3));

    [GeneratedRegex(@"\++\s*$")]
    private static partial Regex TrailingPlus();
}
