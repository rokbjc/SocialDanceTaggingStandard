using SocialDanceTaggingStandard.Taxonomy.Profile;

namespace SocialDanceTaggingStandard.Taxonomy.Ingest;

/// <summary>The projected SDTS view of a track's idiosyncratic tags, keyed by dimension.</summary>
public sealed record ProjectionResult(IReadOnlyDictionary<string, IReadOnlyList<string>> Projected);

/// <summary>
/// Read-time ingest resolver — projects a DJ's existing tags onto SDTS for display. It lives strictly
/// ABOVE the sync layer: <see cref="Project"/> is a pure function of (vocabulary, rules, input strings)
/// and constructs no sync / hash object, so it structurally cannot rewrite files or touch the baseline
/// (see <c>docs/standard/profile.md</c> §5). Write-time migration is a separate, opt-in concern.
/// </summary>
public sealed class IngestMapper
{
    private readonly SocialDanceVocabulary _vocab;
    private readonly IReadOnlyList<CrossDimensionRule> _rules;
    private readonly Dimension? _dances;

    /// <summary>Creates a mapper over a (customized) vocabulary and its cross-dimension rules.</summary>
    public IngestMapper(SocialDanceVocabulary vocab, IReadOnlyList<CrossDimensionRule> crossDimension)
    {
        _vocab = vocab ?? throw new ArgumentNullException(nameof(vocab));
        _rules = crossDimension ?? [];
        _dances = vocab.Packs.Count > 0 ? vocab.Dances([.. vocab.Packs.Keys]) : null;
    }

    private Dimension? DimensionFor(DimensionId dimension)
        => dimension == SocialDanceVocabulary.DancesDimension ? _dances : _vocab.Dimension(dimension);

    /// <summary>
    /// Project the DJ's raw per-dimension tags as SDTS: a same-dimension value resolves alias→canonical
    /// (an unknown value passes through verbatim — never lost); a matching cross-dimension rule emits its
    /// target value under the target dimension, dropping the source only when <c>KeepSource</c> is false.
    /// Pure — performs no writes.
    /// </summary>
    public ProjectionResult Project(IReadOnlyDictionary<string, IReadOnlyList<string>> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        var output = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in tags)
        {
            var srcDim = DimensionFor(new DimensionId(pair.Key));   // pair.Key is an external (DJ-tag) dimension name
            foreach (var raw in pair.Value ?? [])
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                var matched = MatchRules(pair.Key, raw);
                foreach (var rule in matched)
                    Emit(rule.ToDimension, rule.ToValue);   // all matching targets, not just the first
                // keep the source unless a matching rule consumes it (on_source: drop)
                if (matched.Count == 0 || matched.All(r => r.KeepSource))
                    Emit(pair.Key, srcDim?.Resolve(raw)?.Canonical ?? raw);
            }
        }

        return new ProjectionResult(
            output.ToDictionary(kv => kv.Key, IReadOnlyList<string> (kv) => kv.Value, StringComparer.OrdinalIgnoreCase));

        void Emit(string dim, string value)
        {
            if (!output.TryGetValue(dim, out var list))
                output[dim] = list = [];
            if (!list.Contains(value, StringComparer.OrdinalIgnoreCase))
                list.Add(value);
        }
    }

    /// <summary>
    /// Compute the write-time migration plan for the supplied tags: every cross-dimension rule with
    /// <c>Migrate = true</c> that matches a source value contributes a canonical write to its target
    /// dimension (and a removal of the source token when <c>on_source: drop</c>). The consumer APPLIES the
    /// plan through the normal canonical write path, so a migrated value hashes byte-identically to a
    /// native-canonical write. (Same-dimension alias→canonical migration is plain normalisation — the
    /// write-side <c>RoundTripMode</c> — not a cross-dimension concern.) Pure: computes a plan, writes nothing.
    /// </summary>
    public MigrationPlan PlanMigration(IReadOnlyDictionary<string, IReadOnlyList<string>> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);

        var writes = new List<FieldWrite>();
        var removals = new List<FieldWrite>();
        foreach (var pair in tags)
        {
            foreach (var raw in pair.Value ?? [])
            {
                if (string.IsNullOrWhiteSpace(raw))
                    continue;
                foreach (var rule in MatchRules(pair.Key, raw))
                {
                    if (!rule.Migrate) continue;
                    writes.Add(new FieldWrite(rule.ToDimension, rule.ToValue));
                    if (!rule.KeepSource) removals.Add(new FieldWrite(pair.Key, raw));
                }
            }
        }
        return new MigrationPlan(Dedup(writes), Dedup(removals));
    }

    private static IReadOnlyList<FieldWrite> Dedup(List<FieldWrite> items)
    {
        var seen = new HashSet<(string, string)>();
        var ordered = new List<FieldWrite>();
        foreach (var w in items)
        {
            if (seen.Add((w.Dimension.ToLowerInvariant(), w.Value))) ordered.Add(w);
        }

        return ordered;
    }

    private List<CrossDimensionRule> MatchRules(string dim, string value)
    {
        var hits = new List<CrossDimensionRule>();
        foreach (var r in _rules)
        {
            if (!string.Equals(r.FromDimension, dim, StringComparison.OrdinalIgnoreCase)) continue;
            var cmp = r.Match == MatchMode.Exact ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if (string.Equals(r.FromValue, value, cmp))
            {
                hits.Add(r);
            }
        }
        return hits;
    }
}
