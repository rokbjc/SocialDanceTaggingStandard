using System.Globalization;
using System.Text.RegularExpressions;

namespace SocialDanceTaggingStandard.Validation;

/// <summary>
/// Validates a raw stored value against a field's <see cref="FieldConstraints"/>. The .NET side of the
/// cross-language validator; behaviour is pinned by <c>conformance/vectors/validation/fields.json</c>
/// (shared with the Python <c>validate</c>). Per-value only — absent/empty is valid (fields are optional);
/// catalog membership and cross-field invariants are out of scope.
/// </summary>
public static class FieldValidator
{
    /// <summary>
    /// Returns <see cref="FieldValidationResult.Valid"/> when valid; otherwise a failed result whose
    /// <see cref="FieldValidationResult.Error"/> is one of <see cref="ValidationError.NotInteger"/>,
    /// <see cref="ValidationError.OutOfRange"/>, <see cref="ValidationError.NotInEnum"/>,
    /// <see cref="ValidationError.BadGrade"/>, or <see cref="ValidationError.NoMatch"/>.
    /// </summary>
    public static FieldValidationResult Validate(string? value, FieldConstraints constraints)
    {
        if (string.IsNullOrEmpty(value))
        {
            return FieldValidationResult.Valid;
        }

        ValidationError? error = constraints switch
        {
            IntConstraint c => ValidateInt(value, c),
            EnumConstraint c => ValidateParts(value, c.Cardinality, token => ValidateEnumToken(token, c)),
            StringConstraint c => ValidateParts(value, c.Cardinality, token => ValidatePattern(token, c.Pattern)),
            CompositeConstraint c => ValidateParts(value, c.Cardinality, token => ValidatePattern(token, c.Pattern)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(constraints), constraints.GetType(), "Unknown FieldConstraints subtype."),
        };

        return error is { } e ? FieldValidationResult.Fail(e) : FieldValidationResult.Valid;
    }

    private static ValidationError? ValidateInt(string value, IntConstraint c)
    {
        if (!int.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int number))
        {
            return ValidationError.NotInteger;
        }

        if (c.Min is { } min && number < min)
        {
            return ValidationError.OutOfRange;
        }

        if (c.Max is { } max && number > max)
        {
            return ValidationError.OutOfRange;
        }

        return null;
    }

    /// <summary>
    /// Splits <paramref name="value"/> into parts (multi values on the fixed <c>"; "</c> delimiter, single
    /// values as one part) and returns the first per-token error, or <c>null</c> if all pass.
    /// </summary>
    private static ValidationError? ValidateParts(
        string value, string? cardinality, Func<string, ValidationError?> validateToken)
    {
        string[] parts = cardinality == "multi"
            ? value.Split(["; "], StringSplitOptions.None)
            : [value];

        foreach (string part in parts)
        {
            if (validateToken(part.Trim()) is { } error)
            {
                return error;
            }
        }

        return null;
    }

    /// <summary>
    /// Strips the optional trailing <c>?</c> marker and the inline <c>:grade</c> token, then checks intrinsic
    /// enum membership. Returns an error or <c>null</c> when the token is valid.
    /// </summary>
    private static ValidationError? ValidateEnumToken(string token, EnumConstraint c)
    {
        if (c.AllowValidationMarker && token.EndsWith('?'))
        {
            token = token[..^1].Trim();
        }

        if (c.Grades is { Count: > 0 } && token.Contains(':'))
        {
            int idx = token.LastIndexOf(':');
            string grade = token[(idx + 1)..];
            if (!c.Grades.Contains(grade))
            {
                return ValidationError.BadGrade;
            }

            token = token[..idx].Trim();
        }

        if (c.Values is not null && !c.Values.Contains(token))
        {
            return ValidationError.NotInEnum;
        }

        return null;
    }

    private static ValidationError? ValidatePattern(string token, string? pattern) =>
        pattern is not null && !Regex.IsMatch(token, "^(?:" + pattern + ")$")
            ? ValidationError.NoMatch
            : null;
}
