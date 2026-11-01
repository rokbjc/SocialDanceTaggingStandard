namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// An inclusive tempo band in beats per minute — a value's danceable / typical BPM window
/// (e.g. a dance's playable range, or a genre's characteristic tempo). A value type, so it can
/// never be null where present; absence is modelled as a nullable <see cref="BpmRange"/>.
/// </summary>
public readonly record struct BpmRange(int Min, int Max)
{
    /// <inheritdoc/>
    public override string ToString() => $"{Min}–{Max}";
}
