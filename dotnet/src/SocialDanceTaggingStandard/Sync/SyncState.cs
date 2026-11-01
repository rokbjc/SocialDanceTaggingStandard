namespace SocialDanceTaggingStandard.Sync;

/// <summary>
/// The per-field baseline recorded at the last successful sync — the content hash both sides
/// agreed on. It lets the next sync tell which side (if any) changed, enabling a 3-way merge
/// instead of a blind overwrite.
/// <para>
/// Serialized compactly as <c>field=hash; field=hash</c> in a single tag (the same shape as the
/// SDTS <c>confirmation_gates</c> frame), so it costs one frame regardless of field count.
/// </para>
/// </summary>
public sealed class SyncState
{
    private const char EntrySeparator = ';';
    private const char KeyValueSeparator = '=';

    private readonly Dictionary<string, string> _baseline;

    /// <summary>Creates an empty state (nothing synced yet).</summary>
    public SyncState() => _baseline = new Dictionary<string, string>(StringComparer.Ordinal);

    private SyncState(Dictionary<string, string> baseline) => _baseline = baseline;

    /// <summary>The last-synced content hash for a field, or <see langword="null"/> if it was never synced.</summary>
    public string? BaselineHash(FieldId field) => _baseline.GetValueOrDefault(field.Value);

    /// <summary>Records the agreed content hash for a field.</summary>
    public void Set(FieldId field, string hash) => _baseline[field.Value] = hash;

    /// <summary>Forgets a field's baseline.</summary>
    public void Remove(FieldId field) => _baseline.Remove(field.Value);

    /// <summary>Parses the compact <c>field=hash; field=hash</c> form. Null/blank yields a fresh state.</summary>
    public static SyncState Parse(string? encoded)
    {
        var baseline = new Dictionary<string, string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(encoded)) return new SyncState(baseline);
        foreach (string entry in encoded.Split(EntrySeparator))
        {
            string trimmed = entry.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            int sep = trimmed.IndexOf(KeyValueSeparator);
            if (sep <= 0)
            {
                continue;
            }

            string key = trimmed[..sep].Trim();
            string value = trimmed[(sep + 1)..].Trim();
            if (key.Length > 0)
            {
                baseline[key] = value;
            }
        }

        return new SyncState(baseline);
    }

    /// <summary>Renders the compact <c>field=hash; field=hash</c> form with a stable field order.</summary>
    public string Encode() =>
        string.Join(
            "; ",
            _baseline
                .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                .Select(kv => kv.Key + KeyValueSeparator + kv.Value));
}
