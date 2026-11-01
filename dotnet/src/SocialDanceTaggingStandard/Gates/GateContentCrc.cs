using System.Globalization;
using System.Text;

namespace SocialDanceTaggingStandard.Gates;

/// <summary>
/// CRC primitives for the gate integrity stamp. The CRC is taken over a gated field's content so a later
/// load can detect that <em>someone else</em> (MM5, another DJ, a sync tool) changed it after we confirmed
/// it — a change we didn't make, so <c>GateInvalidator</c> never cleared the gate. CRC32 truncated to
/// its low 16 bits (<see cref="HexLength"/> = 4 hex digits): this is tamper *detection*, not cryptographic
/// integrity, so a ~1/65536 chance of a content change colliding to the same stamp is fine, and it keeps the
/// Custom10 string short. Per-gate canonicalization lives in <c>GateContent</c>; this class only
/// turns a canonical string into a CRC.
/// </summary>
public static class GateContentCrc
{
    /// <summary>Number of hex digits in a gate CRC stamp (low 16 bits of CRC32 = 4 hex).</summary>
    public const int HexLength = 4;

    private static readonly uint[] Table = BuildTable();

    /// <summary>Lowercase 4-hex-digit stamp (low 16 bits of the CRC32) of a canonical content string
    /// (trimmed). Null/empty ⇒ a stable stamp of the empty string, so "confirmed empty" round-trips too.</summary>
    /// <param name="canonical">The already-canonicalized content (see <c>GateContent</c>).</param>
    public static string Hex(string? canonical)
    {
        var bytes = Encoding.UTF8.GetBytes((canonical ?? string.Empty).Trim());
        return (Crc32(bytes) & 0xFFFFu).ToString("x4", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Canonical form of a (possibly multi-value) text field: split on <c>;</c> / <c>,</c>, trim, drop
    /// empties, lowercase (invariant), de-duplicate, sort ordinally, rejoin with "; ". Two fields with the
    /// same semantic value produce the same string regardless of order or casing — the same "did the value
    /// actually change?" notion <c>GateInvalidator</c> uses, so a benign reorder/re-casing by an
    /// external editor is NOT flagged as tampering. Only for multi-value TEXT gates — NOT for structured
    /// JSON content (Beats/Structure/Segments), which must be checksummed raw.
    /// </summary>
    /// <param name="content">The raw multi-value field content (e.g. "Big Band Swing; Jump Blues").</param>
    public static string NormalizeSet(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var parts = content
            .Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => p.ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        parts.Sort(StringComparer.Ordinal);
        return string.Join("; ", parts);
    }

    /// <summary>Computes the CRC32 (IEEE / zlib polynomial 0xEDB88320) of a byte span.</summary>
    /// <param name="data">The bytes to checksum.</param>
    public static uint Crc32(ReadOnlySpan<byte> data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
            crc = (crc >> 8) ^ Table[(crc ^ b) & 0xFF];
        return crc ^ 0xFFFFFFFFu;
    }

    private static uint[] BuildTable()
    {
        var table = new uint[256];
        for (var i = 0u; i < 256u; i++)
        {
            var c = i;
            for (var k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            table[i] = c;
        }
        return table;
    }
}
