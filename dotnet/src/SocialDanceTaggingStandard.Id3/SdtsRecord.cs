using System.Globalization;
using SocialDanceTaggingStandard.Gates;
using SocialDanceTaggingStandard.Sync;

namespace SocialDanceTaggingStandard.Id3;

/// <summary>
/// The typed, storage-agnostic SDTS record — typed per-field accessors (generated in
/// <c>SdtsRecordAccessors.g.cs</c>) over a backing <see cref="ISdtsSource"/> (the SDTS_* TXXX region).
/// Container-less: standard frames (genre/mood/bpm/rating), audio, and <c>Save</c> live on the
/// file-backed <see cref="SdtsFile"/> subtype. Non-sealed so a consumer can derive (e.g. a downstream
/// <c>SgdtsRecord : SdtsRecord</c>) and add typed accessors over extra <c>TXXX:SG_*</c> keys through the
/// inherited backend.
/// </summary>
public partial class SdtsRecord : ISdtsSource
{
    /// <summary>The raw backend the typed accessors read/write (the SDTS_* TXXX region).</summary>
    protected ISdtsSource Source { get; }

    /// <summary>Creates a record over any <see cref="ISdtsSource"/>.</summary>
    public SdtsRecord(ISdtsSource source) => Source = source ?? throw new ArgumentNullException(nameof(source));

    // ISdtsSource passthrough → arbitrary TXXX by key.
    /// <inheritdoc />
    public string? Get(TagKey key) => Source.Get(key);
    /// <inheritdoc />
    public void Set(TagKey key, string? value) => Source.Set(key, value);

    /// <summary>Reads a field by its <see cref="FieldId"/> from the SDTS_* TXXX region (SDTS_-backed
    /// fields only; genre/mood/bpm/rating live on <see cref="SdtsFile.Container"/>).</summary>
    public string? GetField(FieldId id) => FieldFrames.TryGetValue(id, out var d) ? Source.Get(new TagKey(d.Value)) : null;
    /// <summary>Writes a field by its <see cref="FieldId"/>.</summary>
    public void SetField(FieldId id, string? value) { if (FieldFrames.TryGetValue(id, out var d)) Source.Set(new TagKey(d.Value), value); }

    // ── reusable typed primitives (base uses them; subclasses reuse for SG_* keys) ──
    /// <summary>Reads a "; "-joined multi-value field.</summary>
    protected IReadOnlyList<string> GetMulti(Id3UserTextDescription key)
    {
        var raw = Source.Get(new TagKey(key.Value));
        return string.IsNullOrEmpty(raw) ? [] : [.. raw.Split("; ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    }
    /// <summary>Writes a multi-value field, joined with "; " (empty clears).</summary>
    protected void SetMulti(Id3UserTextDescription key, IEnumerable<string> values)
        => Source.Set(new TagKey(key.Value), string.Join("; ", values));

    /// <summary>Reads a single-value string field.</summary>
    protected string? GetScalar(Id3UserTextDescription key) => Source.Get(new TagKey(key.Value));
    /// <summary>Writes a single-value string field.</summary>
    protected void SetScalar(Id3UserTextDescription key, string? value) => Source.Set(new TagKey(key.Value), value);

    /// <summary>Reads an integer field; null when absent or non-numeric.</summary>
    protected int? GetInt(Id3UserTextDescription key)
        => int.TryParse(Source.Get(new TagKey(key.Value)), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;
    /// <summary>Writes an integer field (null clears).</summary>
    protected void SetInt(Id3UserTextDescription key, int? value)
        => Source.Set(new TagKey(key.Value), value?.ToString(CultureInfo.InvariantCulture));

    /// <summary>Reads a millisecond field as a <see cref="TimeSpan"/>.</summary>
    protected TimeSpan? GetMs(Id3UserTextDescription key) => GetInt(key) is { } ms ? TimeSpan.FromMilliseconds(ms) : null;
    /// <summary>Writes a <see cref="TimeSpan"/> as integer milliseconds.</summary>
    protected void SetMs(Id3UserTextDescription key, TimeSpan? value)
        => SetInt(key, value is { } t ? (int)t.TotalMilliseconds : null);

    /// <summary>Reads the confirmation-gate ledger (keyed by field id; unknown tokens preserved).</summary>
    protected ConfirmationGates GetGates(Id3UserTextDescription key) => ConfirmationGates.Parse(Source.Get(new TagKey(key.Value))).Gates;
    /// <summary>Writes the confirmation-gate ledger.</summary>
    protected void SetGates(Id3UserTextDescription key, ConfirmationGates value)
    {
        var s = value.Serialize();
        Source.Set(new TagKey(key.Value), string.IsNullOrEmpty(s) ? null : s);
    }
}
