using System.Collections.Frozen;

namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>One group of a closed grouping: the key terms carry in their grouping attribute, and its display label.</summary>
public sealed record TermGroup(string Key, string Label);

/// <summary>The closed, ordered set of groups one term attribute files a dimension's terms under.</summary>
public sealed class TermGrouping
{
    private readonly FrozenDictionary<string, TermGroup> _byKey;

    /// <exception cref="SdtsVocabularyException">No groups are given, or a key repeats.</exception>
    public TermGrouping(string attribute, IReadOnlyList<TermGroup> groups)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attribute);
        ArgumentNullException.ThrowIfNull(groups);
        if (groups.Count == 0)
        {
            throw new SdtsVocabularyException($"Grouping by '{attribute}' declares no groups.");
        }

        var byKey = new Dictionary<string, TermGroup>(StringComparer.Ordinal);
        foreach (var group in groups)
        {
            if (!byKey.TryAdd(group.Key, group))
            {
                throw new SdtsVocabularyException($"Grouping by '{attribute}' declares '{group.Key}' more than once.");
            }
        }

        Attribute = attribute;
        Groups = [.. groups];
        _byKey = byKey.ToFrozenDictionary(StringComparer.Ordinal);
    }

    /// <summary>The term attribute (its YAML key) that holds a group key.</summary>
    public string Attribute { get; }

    /// <summary>The groups in declared order.</summary>
    public IReadOnlyList<TermGroup> Groups { get; }

    /// <summary>The group the term is filed under, or null when its attribute is absent or names no declared group.</summary>
    public TermGroup? GroupOf(VocabularyTerm term)
    {
        ArgumentNullException.ThrowIfNull(term);
        return term.Attributes.TryGetValue(Attribute, out var value) && value is string key
            ? _byKey.GetValueOrDefault(key)
            : null;
    }
}
