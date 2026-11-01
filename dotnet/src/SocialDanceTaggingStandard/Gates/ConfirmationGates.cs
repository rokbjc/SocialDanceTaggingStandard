using System.Collections.Immutable;
using System.Globalization;

namespace SocialDanceTaggingStandard.Gates;

/// <summary>
/// Per-track confirmation-gate layer — which SDTS fields have been human-confirmed, at which pipeline
/// version, and (optionally) the CRC of the confirmed content. Keyed by <see cref="FieldId"/> (like
/// <c>Sync.SyncState</c>): a gate records a confirmation ON A FIELD. Serialized as a semicolon-separated
/// "field:version" or "field:version:crc" string — the SDTS home is <c>TXXX:SDTS_CONFIRMATION_GATES</c>;
/// the MM5 <c>COMM:Songs-DB_Custom10</c> mirror is legacy. Carrying the version makes outdated confirmations
/// detectable after a pipeline version bump; the CRC makes external tampering detectable on load.
///
/// <para>
/// The gate vocabulary is OPEN — any field is gate-able (a core field or a consumer's extension field), so
/// there is no fixed gate list. The wire token is the field's <see cref="FieldId.Value"/>; legacy or alias
/// spellings (e.g. an MM5 catalog's <c>Genres</c> ⇒ <c>genre</c>, <c>Occasions</c> ⇒ <c>dances</c>) are
/// resolved by a caller-supplied <see cref="GateKeyResolver"/> — never hard-coded here. A token that is
/// neither aliased nor a canonical field id is preserved verbatim (no round-trip loss) and reported via
/// <see cref="GateParse.UnrecognizedTokens"/>. Missing keys mean "not confirmed" — never write <c>field:0</c>.
/// </para>
/// </summary>
public sealed record ConfirmationGates
{
    /// <summary>Empty — nothing confirmed.</summary>
    public static ConfirmationGates Empty { get; } = new(ImmutableDictionary<FieldId, GateEntry>.Empty);

    private readonly ImmutableDictionary<FieldId, GateEntry> _entries;

    private ConfirmationGates(ImmutableDictionary<FieldId, GateEntry> entries) => _entries = entries;

    /// <summary>Returns the version this field's gate was confirmed at, or null if the field is not gated.</summary>
    public int? Get(FieldId field) => _entries.TryGetValue(field, out var e) ? e.Version : null;

    /// <summary>Returns the CRC stamped on this field's gate, or null if not gated or stamped without a CRC.</summary>
    public string? GetCrc(FieldId field) => _entries.TryGetValue(field, out var e) ? e.Crc : null;

    /// <summary>Returns the full entry (version + optional CRC) for this field's gate, or null if not gated.</summary>
    public GateEntry? GetEntry(FieldId field) => CollectionExtensions.GetValueOrDefault(_entries, field);

    /// <summary>True iff <paramref name="field"/> is gated (at any version).</summary>
    public bool IsConfirmed(FieldId field) => _entries.ContainsKey(field);

    /// <summary>
    /// Returns a new instance with <paramref name="field"/> gated at <paramref name="version"/> and an optional
    /// content CRC. Any field is gate-able; the only constraint is a well-formed wire token — the field id must
    /// not contain the reserved <c>;</c> / <c>:</c> delimiters.
    /// </summary>
    /// <param name="field">The field being confirmed.</param>
    /// <param name="version">Pipeline version (positive).</param>
    /// <param name="crc">Optional lowercase <see cref="GateContentCrc.HexLength"/>-hex CRC of the gated field's normalized content.</param>
    public ConfirmationGates WithGate(FieldId field, int version, string? crc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field.Value);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        if (field.Value.AsSpan().IndexOfAny(';', ':') >= 0)
            throw new ArgumentException(
                $"Gate field id '{field.Value}' contains a reserved delimiter (';' or ':').", nameof(field));
        return new ConfirmationGates(_entries.SetItem(field, new GateEntry(version, NormalizeCrc(crc))));
    }

    /// <summary>Returns a new instance with <paramref name="field"/>'s gate removed.</summary>
    public ConfirmationGates WithoutGate(FieldId field) =>
        _entries.ContainsKey(field) ? new ConfirmationGates(_entries.Remove(field)) : this;

    /// <summary>All gated fields, in canonical (ordinal-by-id) order.</summary>
    public IEnumerable<FieldId> ConfirmedFields => Ordered.Select(kv => kv.Key);

    /// <summary>All (field, version) pairs, in canonical (ordinal-by-id) order.</summary>
    public IEnumerable<KeyValuePair<FieldId, int>> Entries =>
        Ordered.Select(kv => new KeyValuePair<FieldId, int>(kv.Key, kv.Value.Version));

    private IEnumerable<KeyValuePair<FieldId, GateEntry>> Ordered =>
        _entries.OrderBy(kv => kv.Key.Value, StringComparer.Ordinal);

    /// <summary>
    /// Serialize to the "field:version" / "field:version:crc" wire format, semicolon-separated. Tokens are the
    /// field ids (<see cref="FieldId.Value"/>) in deterministic ordinal order; the CRC suffix is written only
    /// when present. Empty gates ⇒ empty string.
    /// </summary>
    public string Serialize() =>
        _entries.Count == 0
            ? string.Empty
            : string.Join("; ", Ordered.Select(kv => FormatEntry(kv.Key.Value, kv.Value)));

    private static string FormatEntry(string token, GateEntry entry) =>
        string.IsNullOrEmpty(entry.Crc)
            ? FormattableString.Invariant($"{token}:{entry.Version}")
            : FormattableString.Invariant($"{token}:{entry.Version}:{entry.Crc}");

    /// <summary>
    /// Parse "field:version" or "field:version:crc" entries separated by semicolons, resolving each wire token
    /// to a <see cref="FieldId"/>: first via <paramref name="aliases"/> (legacy/extension spellings), then as a
    /// canonical field id (<see cref="FieldIds.ByValue"/>), else preserved verbatim as an opaque field id — so
    /// nothing is lost on round-trip, and reported in <see cref="GateParse.UnrecognizedTokens"/> so it can be
    /// remediated by adding a profile alias. Version must be a positive integer; entries with a non-numeric or
    /// non-positive version are skipped. A malformed CRC (not <see cref="GateContentCrc.HexLength"/> hex digits)
    /// is dropped to null, leaving the gate intact.
    /// </summary>
    /// <param name="raw">The raw tag value (null/blank ⇒ empty, everything recognized).</param>
    /// <param name="aliases">Optional wire-token ⇒ field resolver (typically a personal profile's); null ⇒ canonical-only.</param>
    public static GateParse Parse(string? raw, GateKeyResolver? aliases = null)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return new GateParse(Empty, []);
        }

        var builder = ImmutableDictionary.CreateBuilder<FieldId, GateEntry>();
        var unrecognized = new List<string>();
        foreach (var part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            // "token:version" or "token:version:crc" — at most 3 fields so a CRC can't be confused with extra
            // colons (our hex CRC has none, but stay defensive).
            var parts = part.Split(':', 3, StringSplitOptions.TrimEntries);
            if (parts.Length < 2)
                continue;
            var token = parts[0];
            if (token.Length == 0)
                continue;
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var version))
                continue;
            if (version <= 0)
                continue;
            var crc = parts.Length == 3 ? NormalizeCrc(parts[2]) : null;

            var field = Resolve(token, aliases, out var recognized);
            if (!recognized)
            {
                unrecognized.Add(token);
            }

            builder[field] = new GateEntry(version, crc);
        }

        var gates = builder.Count == 0 ? Empty : new ConfirmationGates(builder.ToImmutable());
        IReadOnlyList<string> unrecognizedTokens = unrecognized;
        return new GateParse(gates, unrecognizedTokens);
    }

    // Token → FieldId: profile alias first, then the exact canonical id, else an opaque preserved id.
    private static FieldId Resolve(string token, GateKeyResolver? aliases, out bool recognized)
    {
        if (aliases?.Invoke(token) is { } aliased)
        {
            recognized = true;
            return aliased;
        }

        if (FieldIds.ByValue.TryGetValue(token, out var canonical))
        {
            recognized = true;
            return canonical;
        }

        recognized = false;
        return new FieldId(token); // preserved for round-trip; flagged for profile remediation
    }

    /// <summary>Normalizes a CRC to a lowercase <see cref="GateContentCrc.HexLength"/>-hex string, or null if it
    /// isn't a valid stamp. A wrong-length stamp drops to null, so the gate is treated as unstamped/intact
    /// rather than spuriously tampered.</summary>
    private static string? NormalizeCrc(string? crc)
    {
        if (string.IsNullOrWhiteSpace(crc))
        {
            return null;
        }

        var trimmed = crc.Trim();
        if (trimmed.Length != GateContentCrc.HexLength)
        {
            return null;
        }

        return trimmed.Any(c => !Uri.IsHexDigit(c)) ? null : trimmed.ToLowerInvariant();
    }
}
