namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// A dance-family vocabulary pack (e.g. <c>"swing"</c>, <c>"blues"</c>) layered on top of the
/// shared core. A pack contributes only the DANCE dimension for its families; every music
/// dimension (genre, mood, feel, …) is shared and lives in <see cref="SocialDanceVocabulary.Core"/>.
/// </summary>
public sealed class DancePack
{
    /// <summary>Creates a dance pack.</summary>
    public DancePack(
        PackId name,
        string displayName,
        string description,
        IReadOnlyList<string> families,
        Dimension dances,
        ReviewStatus reviewStatus)
    {
        Name = name;
        DisplayName = displayName;
        Description = description;
        Families = families;
        Dances = dances;
        ReviewStatus = reviewStatus;
    }

    /// <summary>Pack id, e.g. <c>swing</c>.</summary>
    public PackId Name { get; }

    /// <summary>Human-readable pack name, e.g. <c>"Swing"</c>.</summary>
    public string DisplayName { get; }

    /// <summary>One-line description of what the pack covers.</summary>
    public string Description { get; }

    /// <summary>The catalog dance families this pack bundles, e.g. <c>swing-partner</c>, <c>solo-rhythmic</c>.</summary>
    public IReadOnlyList<string> Families { get; }

    /// <summary>The dances this pack defines.</summary>
    public Dimension Dances { get; }

    /// <summary>Whether SDTS flags this pack's dances as awaiting expert review (its pack.yaml <c>validation</c>).</summary>
    public ReviewStatus ReviewStatus { get; }
}
