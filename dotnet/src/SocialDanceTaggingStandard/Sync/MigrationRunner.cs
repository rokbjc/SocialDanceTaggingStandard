namespace SocialDanceTaggingStandard.Sync;

/// <summary>
/// Applies a set of <see cref="FrameMigration"/> key-renames to a single <see cref="ISdtsSource"/>.
/// Idempotent by frame-state: a migration is a no-op when the source key is empty or the target key is
/// already populated, so re-running on an already-migrated file changes nothing. Value-agnostic except
/// for each migration's optional <see cref="FrameMigration.Accept"/> guard.
/// </summary>
public static class MigrationRunner
{
    /// <summary>Applies <paramref name="migrations"/> to <paramref name="source"/> and returns what happened to each.</summary>
    public static IReadOnlyList<MigrationResult> Apply(ISdtsSource source, IEnumerable<FrameMigration> migrations)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(migrations);

        var results = new List<MigrationResult>();
        foreach (FrameMigration mig in migrations)
        {
            string? from = source.Get(mig.FromKey);
            if (string.IsNullOrEmpty(from))
            {
                results.Add(new MigrationResult(mig.FromKey, mig.ToKey, MigrationOutcome.SkippedEmpty, null));
                continue;
            }

            string? existingTarget = source.Get(mig.ToKey);
            if (!string.IsNullOrEmpty(existingTarget))
            {
                results.Add(new MigrationResult(mig.FromKey, mig.ToKey, MigrationOutcome.SkippedTargetPresent, from));
                continue;
            }

            if (mig.Accept is not null && !mig.Accept(from))
            {
                results.Add(new MigrationResult(mig.FromKey, mig.ToKey, MigrationOutcome.SkippedRejectedValue, from));
                continue;
            }

            source.Set(mig.ToKey, from);
            source.Set(mig.FromKey, null);
            results.Add(new MigrationResult(mig.FromKey, mig.ToKey, MigrationOutcome.Moved, from));
        }

        return results;
    }
}
