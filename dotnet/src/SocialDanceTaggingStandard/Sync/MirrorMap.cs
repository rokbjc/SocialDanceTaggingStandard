namespace SocialDanceTaggingStandard.Sync;

/// <summary>
/// Helpers for composing and validating <see cref="FieldMirror"/> maps. A downstream consumer can
/// extend a standard map (e.g. the MediaMonkey one) with its OWN private fields and reuse the same
/// sync engine; composition fails fast on a colliding field id, canonical key, or mirror key.
/// </summary>
public static class MirrorMap
{
    /// <summary>Concatenates two maps into one validated map — the standard set plus downstream extras.</summary>
    public static IReadOnlyList<FieldMirror> Compose(
        IEnumerable<FieldMirror> first, IEnumerable<FieldMirror> second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        var list = new List<FieldMirror>(first);
        list.AddRange(second);
        Validate(list);
        return list;
    }

    /// <summary>Throws <see cref="ArgumentException"/> if any field id, canonical key, or mirror key repeats.</summary>
    public static void Validate(IReadOnlyList<FieldMirror> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        EnsureUnique("field id", map.Select(m => m.Field));
        EnsureUnique("canonical key", map.Select(m => m.CanonicalKey));
        EnsureUnique("mirror key", map.SelectMany(m => m.MirrorKeys));
    }

    private static void EnsureUnique<T>(string what, IEnumerable<T> keys)
    {
        IGrouping<T, T>? dup = keys
            .GroupBy(k => k)
            .FirstOrDefault(g => g.Count() > 1);

        if (dup is not null)
        {
            throw new ArgumentException($"Duplicate {what} in mirror map: '{dup.Key}'.");
        }
    }
}
