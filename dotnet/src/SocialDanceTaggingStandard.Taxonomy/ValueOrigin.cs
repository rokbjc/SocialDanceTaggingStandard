namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// How a value entered a field, so a writer can leave untouched values byte-exact and only
/// (re)write the ones the user added or edited. <see cref="Stored"/> is what was read from disk;
/// the editor flips entries to <see cref="Added"/>/<see cref="Edited"/> as the user works
/// (e.g. <c>value with { Origin = ValueOrigin.Edited }</c>).
/// </summary>
public enum ValueOrigin
{
    /// <summary>Read from disk, untouched by the current edit session.</summary>
    Stored,
    /// <summary>Newly added by the user in the current edit session.</summary>
    Added,
    /// <summary>An existing value the user changed in the current edit session.</summary>
    Edited,
}
