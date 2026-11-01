namespace SocialDanceTaggingStandard.Sync;

/// <summary>
/// A minimal read/write view over one "side" of a sync — a tag container addressed by opaque
/// <see cref="TagKey"/>s. The canonical SDTS fields, an external mirror, and the sync-state store are
/// each an <see cref="ISdtsSource"/>.
/// <para>
/// Implementations are <b>value-agnostic</b>: they move raw strings and never interpret them. What a
/// key means (a TXXX description, a COMM frame, …) is the implementation's concern, opaque to the sync
/// engine.
/// </para>
/// </summary>
public interface ISdtsSource
{
    /// <summary>Reads a field's raw value, or <see langword="null"/> when absent or empty.</summary>
    string? Get(TagKey key);

    /// <summary>Writes a field's raw value; a <see langword="null"/> or empty value removes it.</summary>
    void Set(TagKey key, string? value);

    /// <summary>Removes a field (convenience for <c>Set(key, null)</c>).</summary>
    void Remove(TagKey key) => Set(key, null);

    /// <summary>True when the field is present (non-null, non-empty).</summary>
    bool Contains(TagKey key) => Get(key) is not null;
}
