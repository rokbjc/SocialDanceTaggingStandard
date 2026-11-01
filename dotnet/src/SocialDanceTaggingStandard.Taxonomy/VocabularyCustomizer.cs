namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// Fluent builder for per-developer vocabulary customization. Obtained from
/// <see cref="SocialDanceVocabulary.Customize"/>; accumulates edits and produces a new vocabulary
/// with <see cref="Build"/> (the baseline is never mutated). Example:
/// <code>
/// var vocab = SocialDanceVocabulary.LoadDefault()
///     .Customize()
///         .AddAlias(DimensionIds.Genre, "Swing", "My Swing")
///         .RemoveAlias(SocialDanceVocabulary.DancesDimension, "#LH")   // opt out of the legacy '#' convention
///         .AddTerm(DimensionIds.Genre, new VocabularyTerm("balkan-brass", "Balkan Brass", new[]{"Brass BG"}, false, new Dictionary&lt;string,object?&gt;()))
///     .Build();
/// </code>
/// </summary>
public sealed class VocabularyCustomizer
{
    private readonly SocialDanceVocabulary _base;
    private readonly VocabularyOverlay _overlay = new VocabularyOverlay();

    internal VocabularyCustomizer(SocialDanceVocabulary baseline) => _base = baseline;

    /// <summary>Add an alias to a term (by canonical, id, or an existing alias).</summary>
    public VocabularyCustomizer AddAlias(DimensionId dimension, string term, string alias)
    {
        _overlay.AddAlias(dimension, term, alias);
        return this;
    }

    /// <summary>Remove an alias from a dimension.</summary>
    public VocabularyCustomizer RemoveAlias(DimensionId dimension, string alias)
    {
        _overlay.RemoveAlias(dimension, alias);
        return this;
    }

    /// <summary>Add a new term to a core dimension.</summary>
    public VocabularyCustomizer AddTerm(DimensionId dimension, VocabularyTerm term)
    {
        _overlay.AddTerm(dimension, term);
        return this;
    }

    /// <summary>Remove a term (by id or canonical) from a dimension.</summary>
    public VocabularyCustomizer RemoveTerm(DimensionId dimension, string idOrCanonical)
    {
        _overlay.RemoveTerm(dimension, idOrCanonical);
        return this;
    }

    /// <summary>Produce a new vocabulary with the accumulated edits applied.</summary>
    /// <exception cref="SdtsVocabularyException">An edit collides with an existing token
    /// (see <see cref="Dimension"/>), so it could never have resolved.</exception>
    public SocialDanceVocabulary Build() => _base.With(_overlay);
}
