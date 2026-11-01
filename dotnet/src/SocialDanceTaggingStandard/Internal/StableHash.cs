using System.Globalization;
using System.Text;

namespace SocialDanceTaggingStandard.Internal;

/// <summary>
/// Deterministic, process-stable content hash (FNV-1a, 32-bit, rendered as 8 lowercase hex chars).
/// Used as the per-field sync baseline so a later run can tell which side changed.
/// <para>
/// Stable across processes, runs, AND languages (unlike <see cref="string.GetHashCode()"/>, which is
/// randomized). Null and empty hash to the same sentinel, so "absent" round-trips cleanly. This is a
/// normative cross-language algorithm — see <c>docs/standard/stable-hash.md</c>; any port (Python/TS)
/// MUST match it byte-for-byte or 3-way sync silently breaks.
/// </para>
/// </summary>
public static class StableHash
{
    private const string AbsentSentinel = "00000000";

    /// <summary>Computes the stable hash of <paramref name="value"/> (null/empty → the absent sentinel).</summary>
    public static string Of(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return AbsentSentinel;
        }

        const uint offsetBasis = 2166136261;
        const uint prime = 16777619;

        uint hash = offsetBasis;
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        foreach (byte b in bytes)
        {
            hash ^= b;
            hash *= prime;
        }

        return hash.ToString("x8", CultureInfo.InvariantCulture);
    }
}
