namespace SocialDanceTaggingStandard.Taxonomy.Ingest;

/// <summary>How a trailing <c>+</c>-run count maps onto the 1-10 rating family.</summary>
public enum RatingScale
{
    /// <summary>Use the count directly, clamped to 1-10 (the default; 11 plus → 10).</summary>
    Clamp10,

    /// <summary>Map the observed 1..<c>Max</c> plus-count linearly onto 1-10.</summary>
    Linear,
}

/// <summary>What an unrecognised trailing token does. v1 never guesses a dimension for it.</summary>
public enum UnknownTokenPolicy
{
    /// <summary>Leave it in the title; surface it via the token audit (the default).</summary>
    Flag,

    /// <summary>Same as <see cref="Flag"/> for v1 (no per-file guessing). Reserved for a future strict-tail mode.</summary>
    Keep,
}

/// <summary>A trailing <c>+</c>-run rating extractor: <c>+++</c> → a rating value on <see cref="To"/>.</summary>
public sealed record PlusRunRating(string To = "rating", RatingScale Scale = RatingScale.Clamp10, int Max = 11);

/// <summary>A year extractor: the first <see cref="Pattern"/> match in the basename → <see cref="To"/>.</summary>
public sealed record YearExtractor(string To = "recording_year", string Pattern = @"\b(19|20)\d{2}\b");

/// <summary>One tail-token mapping: a recognised token means a value in a dimension. Doubles as the
/// recogniser — a trailing token is "tail" iff it appears here (case-insensitive).</summary>
public sealed record PathCode(string Token, string ToDimension, string ToValue);

/// <summary>
/// One path-glob-scoped rule for projecting a relative path onto SDTS fields. The stable skeleton is a
/// Mp3tag-style <see cref="Mask"/> (<c>%artist% - %title% &lt;tail&gt;</c>); the variable trailing part is
/// resolved by <see cref="Codes"/> + the optional <see cref="Rating"/> / <see cref="Year"/> extractors.
/// </summary>
public sealed record PathRule(
    string Match,
    string Mask,
    IReadOnlyList<PathCode> Codes,
    PlusRunRating? Rating = null,
    YearExtractor? Year = null,
    UnknownTokenPolicy OnUnknownToken = UnknownTokenPolicy.Flag)
{
    private static readonly IReadOnlyDictionary<string, string> NoSet = new Dictionary<string, string>();

    /// <summary>Unconditional field→value writes this rule contributes regardless of the tail (empty if
    /// none); a non-null collection, so consumers never null-check it.</summary>
    public IReadOnlyDictionary<string, string> Set { get; init; } = NoSet;
}
