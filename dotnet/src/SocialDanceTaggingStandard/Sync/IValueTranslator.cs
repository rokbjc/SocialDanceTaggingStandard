namespace SocialDanceTaggingStandard.Sync;

/// <summary>
/// Translates a field's raw value between the canonical SDTS form and an external mirror's
/// form (for example, canonical dance <i>names</i> versus MediaMonkey's legacy <c>#</c>-codes).
/// <para>
/// Absent a value-translation table there is nothing to translate, so the default
/// implementation is <see cref="IdentityValueTranslator"/> (pure pass-through). A
/// taxonomy-backed translator can be supplied later without touching the sync engine —
/// it is the single seam where value-level migration will plug in.
/// </para>
/// </summary>
public interface IValueTranslator
{
    /// <summary>Maps a canonical value to its mirror representation. The return is nullable by contract:
    /// a null/empty <paramref name="canonicalValue"/> maps to null (the field is absent on that side) —
    /// null is a legitimate "no value" pass-through, never an error.</summary>
    string? CanonicalToMirror(FieldId field, string? canonicalValue);

    /// <summary>Maps a mirror value back to its canonical representation. The return is nullable by contract:
    /// a null/empty <paramref name="mirrorValue"/> maps to null (the field is absent on that side) —
    /// null is a legitimate "no value" pass-through, never an error.</summary>
    string? MirrorToCanonical(FieldId field, string? mirrorValue);
}

/// <summary>
/// Pass-through translator: the mirror value equals the canonical value verbatim.
/// This is the only correct behaviour until a value-level taxonomy exists.
/// </summary>
public sealed class IdentityValueTranslator : IValueTranslator
{
    /// <summary>Shared, stateless instance.</summary>
    public static readonly IdentityValueTranslator Instance = new();

    /// <inheritdoc />
    public string? CanonicalToMirror(FieldId field, string? canonicalValue) => canonicalValue;

    /// <inheritdoc />
    public string? MirrorToCanonical(FieldId field, string? mirrorValue) => mirrorValue;
}
