namespace SocialDanceTaggingStandard.Validation;

/// <summary>
/// Machine-checkable per-value validation failure classes. The snake_case wire form
/// (<see cref="ValidationErrorExtensions.ToWireCode"/>) is the cross-language contract pinned by
/// <c>schemas/field-constraints.schema.json</c> and the validation conformance vectors.
/// </summary>
public enum ValidationError
{
    /// <summary>The value is not a base-10 integer.</summary>
    NotInteger,

    /// <summary>The integer is outside the constraint's inclusive range.</summary>
    OutOfRange,

    /// <summary>A token is not a member of the closed enum.</summary>
    NotInEnum,

    /// <summary>An inline <c>:grade</c> token is not an allowed grade.</summary>
    BadGrade,

    /// <summary>A token does not fully match the constraint's pattern.</summary>
    NoMatch,
}

/// <summary>Boundary conversions for <see cref="ValidationError"/>.</summary>
public static class ValidationErrorExtensions
{
    /// <summary>
    /// The stable snake_case wire code. Hand-mapped (not naming-policy-derived) so the code cannot
    /// silently drift from <c>schemas/field-constraints.schema.json</c>; an unmapped future member
    /// throws rather than emitting a wrong string.
    /// </summary>
    public static string ToWireCode(this ValidationError error) => error switch
    {
        ValidationError.NotInteger => "not_integer",
        ValidationError.OutOfRange => "out_of_range",
        ValidationError.NotInEnum => "not_in_enum",
        ValidationError.BadGrade => "bad_grade",
        ValidationError.NoMatch => "no_match",
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, "Unmapped ValidationError."),
    };
}

/// <summary>
/// The outcome of validating one stored value. Backed by a single nullable field: a <c>null</c>
/// <see cref="Error"/> means valid. There is no separate validity flag, so no illegal state is
/// representable — including <c>default</c>, which is <see cref="Valid"/>.
/// </summary>
public readonly record struct FieldValidationResult
{
    private FieldValidationResult(ValidationError? error) => Error = error;

    /// <summary>The failure class, or <c>null</c> when the value is valid.</summary>
    public ValidationError? Error { get; }

    /// <summary>True iff there is no error. Computed, never stored.</summary>
    public bool IsValid => Error is null;

    /// <summary>A valid outcome. Equal to <c>default</c>.</summary>
    public static FieldValidationResult Valid => default;

    /// <summary>An invalid outcome carrying its failure class.</summary>
    public static FieldValidationResult Fail(ValidationError error) => new(error);
}
