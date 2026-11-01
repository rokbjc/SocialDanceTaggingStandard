namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// A per-deployment customization of the baseline vocabulary: aliases to add/remove and terms to
/// add/remove, keyed by <see cref="DimensionId"/> (a core dimension, or
/// <see cref="SocialDanceVocabulary.DancesDimension"/> which applies to every pack). Apply it with
/// <see cref="SocialDanceVocabulary.With"/>; build one fluently via
/// <see cref="SocialDanceVocabulary.Customize"/>; or load one from YAML with
/// <see cref="SocialDanceVocabulary.LoadOverlay(System.IO.FileInfo)"/> (a <c>.sdvo.yaml</c>).
/// </summary>
public sealed class VocabularyOverlay
{
    private static readonly StringComparer Oic = StringComparer.OrdinalIgnoreCase;

    /// <summary>dimension → (term canonical/id/alias → aliases to add).</summary>
    public Dictionary<DimensionId, Dictionary<string, List<string>>> AddAliases { get; } = new();

    /// <summary>dimension → aliases to remove (matched case-insensitively across its terms).</summary>
    public Dictionary<DimensionId, List<string>> RemoveAliases { get; } = new();

    /// <summary>dimension → terms to add (core dimensions only).</summary>
    public Dictionary<DimensionId, List<VocabularyTerm>> AddTerms { get; } = new();

    /// <summary>dimension → term ids/canonicals to remove.</summary>
    public Dictionary<DimensionId, List<string>> RemoveTerms { get; } = new();

    /// <summary>Add an alias to a term (identified by its canonical, id, or an existing alias).</summary>
    public VocabularyOverlay AddAlias(DimensionId dimension, string term, string alias)
    {
        if (!AddAliases.TryGetValue(dimension, out var perTerm))
            AddAliases[dimension] = perTerm = new Dictionary<string, List<string>>(Oic);
        if (!perTerm.TryGetValue(term, out var list))
            perTerm[term] = list = [];
        list.Add(alias);
        return this;
    }

    /// <summary>Remove an alias from a dimension (e.g. opt out of a legacy <c>#</c>-code).</summary>
    public VocabularyOverlay RemoveAlias(DimensionId dimension, string alias)
    {
        Get(RemoveAliases, dimension).Add(alias);
        return this;
    }

    /// <summary>Add a new term to a core dimension.</summary>
    public VocabularyOverlay AddTerm(DimensionId dimension, VocabularyTerm term)
    {
        Get(AddTerms, dimension).Add(term);
        return this;
    }

    /// <summary>Remove a term (by id or canonical) from a dimension.</summary>
    public VocabularyOverlay RemoveTerm(DimensionId dimension, string idOrCanonical)
    {
        Get(RemoveTerms, dimension).Add(idOrCanonical);
        return this;
    }

    /// <summary>
    /// This overlay split into its removals — which cannot collide, so they apply as one batch — and one
    /// single-edit overlay per addition, in application order. Lets a caller apply the additions one at a
    /// time and keep the ones that build (see <see cref="Profile.ProfileValidator"/>).
    /// </summary>
    internal (VocabularyOverlay Removals, IReadOnlyList<(string What, VocabularyOverlay Edit)> Additions) Split()
    {
        var removals = new VocabularyOverlay();
        foreach (var (dimension, aliases) in RemoveAliases)
        {
            foreach (var alias in aliases)
            {
                removals.RemoveAlias(dimension, alias);
            }
        }
        foreach (var (dimension, terms) in RemoveTerms)
        {
            foreach (var term in terms)
            {
                removals.RemoveTerm(dimension, term);
            }
        }

        var additions = new List<(string, VocabularyOverlay)>();
        foreach (var (dimension, perTerm) in AddAliases)
        {
            foreach (var (term, aliases) in perTerm)
            {
                foreach (var alias in aliases)
                {
                    additions.Add(($"alias '{alias}' for '{term}' in '{dimension.Value}'",
                                   new VocabularyOverlay().AddAlias(dimension, term, alias)));
                }
            }
        }
        foreach (var (dimension, terms) in AddTerms)
        {
            foreach (var term in terms)
            {
                additions.Add(($"custom term '{term.Id}' in '{dimension.Value}'",
                               new VocabularyOverlay().AddTerm(dimension, term)));
            }
        }
        return (removals, additions);
    }

    private static List<T> Get<T>(Dictionary<DimensionId, List<T>> map, DimensionId key)
    {
        if (!map.TryGetValue(key, out var list)) map[key] = list = [];
        return list;
    }
}
