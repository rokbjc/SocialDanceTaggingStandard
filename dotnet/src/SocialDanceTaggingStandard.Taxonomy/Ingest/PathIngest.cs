using SocialDanceTaggingStandard.Taxonomy.Profile;

namespace SocialDanceTaggingStandard.Taxonomy.Ingest;

/// <summary>A path projection: the mapped SDTS view plus the trailing tokens left unmapped (audit list).</summary>
public sealed record PathProjection(ProjectionResult Projected, IReadOnlyList<string> Unmapped);

/// <summary>
/// Composes the parse stage (<see cref="PathParser"/>) with the existing map stage
/// (<see cref="IngestMapper"/>): selects the first rule whose glob matches, parses the path, turns the
/// rule's <see cref="PathCode"/> dictionary into <see cref="CrossDimensionRule"/>s (source dimension
/// <c>"path"</c>, consumed on match), and projects. Read-only; constructs no sync object.
/// </summary>
public sealed class PathIngest
{
    private readonly SocialDanceVocabulary _vocab;
    private readonly IReadOnlyList<PathRule> _rules;

    /// <summary>Creates a path-ingest over a (customized) vocabulary and a set of <see cref="PathRule"/>s.</summary>
    public PathIngest(SocialDanceVocabulary vocab, IReadOnlyList<PathRule> rules)
    {
        _vocab = vocab ?? throw new ArgumentNullException(nameof(vocab));
        _rules = rules ?? [];
    }

    /// <summary>Project a relative path onto SDTS, or null if no rule matches.</summary>
    public PathProjection? Project(string relativePath)
    {
        PathRule? rule = _rules.FirstOrDefault(r => PathParser.Matches(r, relativePath));
        if (rule is null) return null;

        PathCapture capture = PathParser.Parse(relativePath, rule)!;
        var cross = rule.Codes
            .Select(c => new CrossDimensionRule("path", c.Token, c.ToDimension, c.ToValue, KeepSource: false))
            .ToList();
        var mapper = new IngestMapper(_vocab, cross);
        return new PathProjection(mapper.Project(capture.Fields), capture.Unmapped);
    }
}
