using System.Text.Json.Serialization;

namespace SocialDanceTaggingStandard.Validation;

/// <summary>
/// Machine-checkable constraints for one field — the value contract the prose <c>allowed_values</c>
/// describes. Authored in <c>data/field-constraints.yaml</c> and embedded in the validation conformance
/// vectors; consumed by <see cref="FieldValidator"/>. Per-value only — vocabulary membership
/// and cross-field rules are out of scope.
/// </summary>
/// <remarks>
/// A closed hierarchy keyed by the authored <c>type</c> discriminator: each field carries exactly the
/// constraints that apply to its kind, so illegal combinations (an integer range on an enum, a regex on a
/// bounded integer) are unrepresentable rather than merely left null. The discriminator is read first when
/// deserializing, which the conformance vectors satisfy by always emitting <c>type</c> as the leading key.
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(IntConstraint), "int")]
[JsonDerivedType(typeof(EnumConstraint), "enum")]
[JsonDerivedType(typeof(StringConstraint), "string")]
[JsonDerivedType(typeof(CompositeConstraint), "composite")]
public abstract record FieldConstraints;

/// <summary>
/// A single integer bounded by an inclusive <see cref="Min"/>/<see cref="Max"/> range (either end may be
/// open). Integers are intrinsically single-valued, so they are never split into parts.
/// </summary>
public sealed record IntConstraint : FieldConstraints
{
    /// <summary>Inclusive minimum; <c>null</c> leaves the range open below.</summary>
    public int? Min { get; init; }

    /// <summary>Inclusive maximum; <c>null</c> leaves the range open above.</summary>
    public int? Max { get; init; }
}

/// <summary>
/// Shared base for the value-list kinds (<c>enum</c>, <c>string</c>, <c>composite</c>). A <c>multi</c>
/// <see cref="Cardinality"/> splits the stored value into parts that are each validated independently;
/// anything else is treated as one part.
/// </summary>
public abstract record ValueListConstraints : FieldConstraints
{
    /// <summary><c>single</c> | <c>multi</c> (multi values are split into parts before validation).</summary>
    public string? Cardinality { get; init; }

    /// <summary>
    /// The authored multi-value delimiter (the SDTS default is <c>"; "</c>). Documented for tooling; the
    /// validator fixes the split at <c>"; "</c> to stay byte-identical with the conformance vectors.
    /// </summary>
    public string? Delimiter { get; init; }
}

/// <summary>
/// A term from a controlled vocabulary. A present <see cref="Values"/> set is intrinsic and closed; when it
/// is omitted the value set is owned by the taxonomy vocabulary layer (out of scope for this per-value
/// validator) and only the shape — inline <c>:grade</c> tokens and an optional trailing <c>?</c> marker —
/// is checked here.
/// </summary>
public sealed record EnumConstraint : ValueListConstraints
{
    /// <summary>Closed intrinsic membership; <c>null</c> when the vocabulary layer owns the values.</summary>
    public IReadOnlyList<string>? Values { get; init; }

    /// <summary>Allowed inline <c>:grade</c> tokens (e.g. <c>core/edge/fusion</c> on dances).</summary>
    public IReadOnlyList<string>? Grades { get; init; }

    /// <summary>Whether a trailing <c>?</c> (needs-validation marker) is allowed and stripped.</summary>
    public bool AllowValidationMarker { get; init; }
}

/// <summary>A free-form string whose every part must fully match <see cref="Pattern"/>.</summary>
public sealed record StringConstraint : ValueListConstraints
{
    /// <summary>Full-match regex each value (part) must satisfy.</summary>
    public string? Pattern { get; init; }
}

/// <summary>
/// A structured token (e.g. a <c>confirmation_gates</c> entry) whose shape — not membership — is pinned by
/// <see cref="Pattern"/>; each part must match fully.
/// </summary>
public sealed record CompositeConstraint : ValueListConstraints
{
    /// <summary>Full-match regex each value (part) must satisfy.</summary>
    public string? Pattern { get; init; }
}
