namespace SocialDanceTaggingStandard.Taxonomy.Ingest;

/// <summary>
/// The structured result of parsing one path: captured <see cref="Fields"/> (artist/title, the rating/year
/// targets, and recognised codes under the synthetic <c>"path"</c> key) plus the <see cref="Unmapped"/>
/// trailing tokens the rule's dictionary did not recognise (the audit list — never guessed).
/// </summary>
public sealed record PathCapture(
    IReadOnlyDictionary<string, IReadOnlyList<string>> Fields,
    IReadOnlyList<string> Unmapped);
