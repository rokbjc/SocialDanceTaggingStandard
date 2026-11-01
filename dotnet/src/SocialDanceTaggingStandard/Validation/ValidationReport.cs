namespace SocialDanceTaggingStandard.Validation;

/// <summary>Whether a validation message blocks validity or is merely advisory.</summary>
public enum ValidationSeverity
{
    /// <summary>A hard error: the subject is invalid.</summary>
    Error,

    /// <summary>Advisory: noted but does not affect validity.</summary>
    Warning,
}

/// <summary>One human-facing validation message. <see cref="Text"/> is always present.</summary>
public sealed record ValidationMessage(ValidationSeverity Severity, string Text);

/// <summary>
/// The outcome of an aggregate validation (e.g. a profile against a baseline): a severity-tagged
/// message list. Valid iff no message has <see cref="ValidationSeverity.Error"/>; warnings never
/// affect validity.
/// </summary>
/// <remarks>
/// The shared, strongly-typed result for aggregate validation. Returned by
/// <c>SocialDanceTaggingStandard.Taxonomy.ProfileValidator.Validate</c>, which replaced its former ad hoc
/// <c>Errors</c>/<c>Warnings</c> string-list report with this type.
/// </remarks>
public sealed record ValidationReport(IReadOnlyList<ValidationMessage> Messages)
{
    /// <summary>True when there are no error-severity messages.</summary>
    public bool IsValid => Messages.All(m => m.Severity != ValidationSeverity.Error);

    /// <summary>Error-severity message texts (a finished snapshot, in message order).</summary>
    public IReadOnlyList<string> Errors =>
        [.. Messages.Where(m => m.Severity == ValidationSeverity.Error).Select(m => m.Text)];

    /// <summary>Warning-severity message texts (a finished snapshot, in message order).</summary>
    public IReadOnlyList<string> Warnings =>
        [.. Messages.Where(m => m.Severity == ValidationSeverity.Warning).Select(m => m.Text)];
}
