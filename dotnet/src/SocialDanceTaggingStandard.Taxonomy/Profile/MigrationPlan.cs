using SocialDanceTaggingStandard.Taxonomy.Ingest;

namespace SocialDanceTaggingStandard.Taxonomy.Profile;

/// <summary>A single planned canonical write (or removal) against a dimension on a track.</summary>
public sealed record FieldWrite(string Dimension, string Value);

/// <summary>
/// The result of <see cref="IngestMapper.PlanMigration"/>: canonical <see cref="Writes"/> to apply and
/// source <see cref="Removals"/> to clear. The consumer applies these through the normal canonical write
/// path (so every migrated value hashes byte-identically to a native-canonical write); the plan itself
/// touches nothing.
/// </summary>
public sealed record MigrationPlan(IReadOnlyList<FieldWrite> Writes, IReadOnlyList<FieldWrite> Removals);
