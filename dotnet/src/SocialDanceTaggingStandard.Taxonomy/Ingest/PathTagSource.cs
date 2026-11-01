using SocialDanceTaggingStandard.Sync;

namespace SocialDanceTaggingStandard.Taxonomy.Ingest;

/// <summary>
/// Exposes a path projection as a read-only <see cref="ISdtsSource"/> — so a file PATH becomes "just
/// another tag source", addressed by SDTS field id (the projection's dimension keys), with multi-value
/// fields joined by the configured delimiter. This lets a path compose with the same <c>Sync</c> seam as a
/// real tag container: a consumer reads SDTS off a path uniformly, and (the deferred, opt-in concern) a
/// migration imports FROM it into the file's canonical frames via the mirror engine.
/// <para>
/// Read-only by construction: <see cref="Set"/> throws. A path is a projection, not a writable container —
/// migration writes go through the file's own canonical source, never back onto the path.
/// </para>
/// </summary>
public sealed class PathTagSource : ISdtsSource
{
    private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _fields;
    private readonly string _delimiter;

    /// <summary>Wrap an already-computed <see cref="ProjectionResult"/>. <paramref name="delimiter"/> is the
    /// multi-value separator used to join a field's values (default <c>"; "</c>).</summary>
    public PathTagSource(ProjectionResult projection, string delimiter = "; ")
    {
        ArgumentNullException.ThrowIfNull(projection);
        _fields = projection.Projected;
        _delimiter = string.IsNullOrEmpty(delimiter) ? "; " : delimiter;
    }

    /// <summary>Project <paramref name="relativePath"/> through <paramref name="ingest"/> and wrap it, or
    /// null when no rule matches (mirroring <see cref="PathIngest.Project"/>).</summary>
    public static PathTagSource? For(PathIngest ingest, string relativePath, string delimiter = "; ")
    {
        ArgumentNullException.ThrowIfNull(ingest);
        PathProjection? projection = ingest.Project(relativePath);
        return projection is null ? null : new PathTagSource(projection.Projected, delimiter);
    }

    /// <summary>The projected (multi-)value for an SDTS field id, joined with the delimiter; null if the
    /// path yielded nothing for that field.</summary>
    public string? Get(TagKey key)
        => _fields.TryGetValue(key.Value, out var values) && values.Count > 0
            ? string.Join(_delimiter, values)
            : null;

    /// <summary>Always throws — a path is a read-only projection (see the type remarks).</summary>
    public void Set(TagKey key, string? value)
        => throw new NotSupportedException("PathTagSource is read-only; a path cannot be written through the tag seam.");
}
