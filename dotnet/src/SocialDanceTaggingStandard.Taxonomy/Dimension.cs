using System.Collections.Frozen;

namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// A taxonomy dimension: an ordered, alias-resolvable set of <see cref="VocabularyTerm"/>s —
/// e.g. all genres, all moods, or the dances contributed by one pack. Beyond lookup it offers the
/// value-level "manager" operations: <see cref="Contains"/> (validation) and <see cref="Normalize"/>
/// (alias-collapse + de-duplication + parent/child hierarchy resolution).
/// </summary>
public sealed class Dimension
{
    private static readonly string[] PurityGrades = ["core", "edge", "fusion"];

    private readonly IReadOnlyDictionary<string, VocabularyTerm> _byKey; // canonical + aliases + display codes (case-insensitive)
    private readonly IReadOnlyDictionary<string, VocabularyTerm> _byId;  // stable id → term

    /// <summary>
    /// Creates a dimension and builds its case-insensitive lookup over canonicals, aliases, and
    /// display codes — the bare short code (<c>"LH"</c>) is as much an input as the prose name.
    /// </summary>
    /// <exception cref="SdtsVocabularyException">An id is defined twice, or two terms claim the same
    /// token ignoring case with no rule to pick between them — see <see cref="ClaimCode"/> for the
    /// one collision that IS decidable and therefore allowed.</exception>
    public Dimension(
        DimensionId name,
        string layer,
        IReadOnlyList<VocabularyTerm> terms,
        IReadOnlyDictionary<string, ReviewStatus>? attributeReview = null,
        TermGrouping? grouping = null)
    {
        ArgumentNullException.ThrowIfNull(terms);
        Name = name;
        Layer = layer;
        Terms = terms;
        Grouping = grouping;
        AttributeReview = (attributeReview ?? FrozenDictionary<string, ReviewStatus>.Empty)
            .ToFrozenDictionary(StringComparer.Ordinal);

        var byKey = new Dictionary<string, VocabularyTerm>(StringComparer.OrdinalIgnoreCase);
        var byId = new Dictionary<string, VocabularyTerm>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in terms)
        {
            if (!byId.TryAdd(t.Id, t))
            {
                throw new SdtsVocabularyException($"{name.Value}: '{t.Id}' is defined more than once.");
            }
            ClaimName(byKey, name, t.Canonical, t);
            foreach (var alias in t.Aliases)
            {
                ClaimName(byKey, name, alias, t);
            }
        }
        var keysHeldByACode = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in terms)
        {
            ClaimCode(byKey, keysHeldByACode, name, t);
        }
        _byKey = byKey;
        _byId = byId;
    }

    /// <summary>Registers a term's canonical or alias — the tokens that outrank a display code.</summary>
    /// <remarks>The message names the colliding terms but never the token itself — in an overlay the
    /// token is the DJ's own text (see <see cref="SdtsVocabularyException"/>).</remarks>
    private static void ClaimName(
        Dictionary<string, VocabularyTerm> byKey,
        DimensionId dimension,
        string token,
        VocabularyTerm term)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return;
        }
        var key = token.Trim();
        if (!byKey.TryGetValue(key, out var owner))
        {
            byKey[key] = term;
            return;
        }
        if (string.Equals(owner.Id, term.Id, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        throw Collision(dimension, "name or alias", term, owner);
    }

    /// <summary>
    /// Registers a term's display code, which claims after every canonical and alias and YIELDS to one
    /// another term already holds. That yield is the rebinding a personal overlay performs — a DJ who
    /// never dances St. Louis Shag binds its code <c>"SLS"</c> to Salsa as an alias, and the code stops
    /// being St. Louis Shag's mirror (see <see cref="TaxonomyValueTranslator"/>). Two display codes
    /// colliding has no such winner, so it is rejected like any other unreachable token.
    /// </summary>
    private static void ClaimCode(
        Dictionary<string, VocabularyTerm> byKey,
        HashSet<string> keysHeldByACode,
        DimensionId dimension,
        VocabularyTerm term)
    {
        var code = term.Display?.Code;
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }
        var key = code.Trim();
        if (!byKey.TryGetValue(key, out var owner))
        {
            byKey[key] = term;
            keysHeldByACode.Add(key);
            return;
        }
        if (string.Equals(owner.Id, term.Id, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        if (!keysHeldByACode.Contains(key))
        {
            return;
        }
        throw Collision(dimension, "display code", term, owner);
    }

    private static SdtsVocabularyException Collision(
        DimensionId dimension, string kind, VocabularyTerm term, VocabularyTerm owner)
        => new SdtsVocabularyException(
            $"{dimension.Value}: a {kind} of '{term.Id}' collides case-insensitively with a token of "
            + $"'{owner.Id}', so one of the two would be unreachable by it.");

    /// <summary>Dimension id, e.g. <c>genre</c>, <c>mood</c>, <c>dances</c>.</summary>
    public DimensionId Name { get; }

    /// <summary><c>"core"</c> (shared music vocabulary) or <c>"pack"</c> (dance vocabulary).</summary>
    public string Layer { get; }

    /// <summary>The terms in catalog order.</summary>
    public IReadOnlyList<VocabularyTerm> Terms { get; }

    /// <summary>Review status per term attribute (its YAML key, e.g. <c>typical_song_forms</c>), from the
    /// dimension's <c>validation:</c> map; an attribute not listed is <see cref="ReviewStatus.Unflagged"/>.</summary>
    public IReadOnlyDictionary<string, ReviewStatus> AttributeReview { get; }

    /// <summary>The closed, ordered set of groups the terms are filed under, or null when the dimension is
    /// flat or hierarchy-only. <see cref="TermGrouping.GroupOf"/> reads the term attribute it names.</summary>
    public TermGrouping? Grouping { get; }

    /// <summary>The terms whose <c>parent</c> is <paramref name="term"/>, in catalog order.</summary>
    public IReadOnlyList<VocabularyTerm> ChildrenOf(VocabularyTerm term)
    {
        ArgumentNullException.ThrowIfNull(term);
        return [.. Terms.Where(t => string.Equals(ParentId(t), term.Id, StringComparison.OrdinalIgnoreCase))];
    }

    /// <summary>
    /// Resolve a canonical name or alias to its term, or null. Case-insensitive and trimmed; the
    /// dance inline markers (a <c>:core</c>/<c>:edge</c>/<c>:fusion</c> purity grade and a trailing
    /// <c>?</c> "needs validation") are stripped first, so <c>"Lindy Hop:edge?"</c> resolves.
    /// </summary>
    public VocabularyTerm? Resolve(string nameOrAlias)
    {
        if (string.IsNullOrWhiteSpace(nameOrAlias)) return null;
        return _byKey.GetValueOrDefault(StripMarkers(nameOrAlias));
    }

    /// <summary>True if the value (canonical or alias, ignoring dance grade/<c>?</c> markers) is in this dimension.</summary>
    public bool Contains(string value) => Resolve(value) != null;

    /// <summary>
    /// Resolve each value to its canonical term and return a de-conflicted set: aliases collapse to
    /// canonical, duplicates are removed, and — where the dimension has a parent hierarchy — an
    /// ancestor is dropped when a more specific descendant is also present (e.g. genre <c>Swing</c>
    /// is dropped when <c>Big Band</c> is present). Unresolvable values are skipped — check with
    /// <see cref="Contains"/> first if you need to detect them. Order follows first appearance.
    /// </summary>
    public IReadOnlyList<VocabularyTerm> Normalize(IEnumerable<string> values)
    {
        var ordered = new List<VocabularyTerm>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in values ?? [])
        {
            if (raw is null) continue;
            var term = Resolve(raw);
            if (term is not null && seen.Add(term.Id)) ordered.Add(term);
        }
        if (ordered.Count < 2) return ordered;

        // Hierarchy dedup: drop an ancestor when a more-specific descendant is present.
        var drop = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var term in ordered)
        {
            var guard = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var parent = ParentId(term); parent is not null && guard.Add(parent); parent = NextParent(parent))
            {
                if (seen.Contains(parent)) drop.Add(parent);
            }
        }
        if (drop.Count == 0) return ordered;

        var result = new List<VocabularyTerm>();
        foreach (var t in ordered)
        {
            if (!drop.Contains(t.Id)) result.Add(t);
        }
        return result;
    }

    private static string? ParentId(VocabularyTerm term)
        => term.Attributes.TryGetValue("parent", out var v) ? v as string : null;

    private string? NextParent(string id)
        => _byId.TryGetValue(id, out var t) ? ParentId(t) : null;

    private static string StripMarkers(string value)
    {
        var v = value.Trim();
        if (v.EndsWith('?')) v = v[..^1].TrimEnd();
        var colon = v.LastIndexOf(':');
        if (colon > 0)
        {
            var suffix = v[(colon + 1)..].Trim();
            foreach (var grade in PurityGrades)
            {
                if (string.Equals(suffix, grade, StringComparison.OrdinalIgnoreCase))
                {
                    return v[..colon].TrimEnd();
                }
            }
        }
        return v;
    }
}
