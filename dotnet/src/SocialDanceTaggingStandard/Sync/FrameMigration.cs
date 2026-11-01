namespace SocialDanceTaggingStandard.Sync;

/// <summary>
/// Declares one historical key rename within a single <see cref="ISdtsSource"/>: the value that used
/// to live under <see cref="FromKey"/> now belongs under <see cref="ToKey"/>. Applied by
/// <see cref="MigrationRunner"/> on load. Value-agnostic except for the optional <see cref="Accept"/>
/// guard, which lets a consumer (e.g. the reference implementation) refuse to import a value that does not match the
/// target field's constraint — so a historically-overloaded or provisionally-mapped frame cannot
/// corrupt the new canonical field.
/// </summary>
/// <param name="FromKey">The legacy key to move from (e.g. <c>SB-Beats</c> or the ad-hoc <c>Quality</c>).</param>
/// <param name="ToKey">The new key to move to (e.g. <c>SG_Beats</c> or <c>SDTS_RECORDING_QUALITY_ERA</c>).</param>
/// <param name="Accept">
/// Optional value gate: when supplied, the value is moved only if <c>Accept(value)</c> is true; otherwise
/// it is left under <see cref="FromKey"/> and reported as <see cref="MigrationOutcome.SkippedRejectedValue"/>.
/// </param>
public sealed record FrameMigration(TagKey FromKey, TagKey ToKey, Func<string, bool>? Accept = null);

/// <summary>Per-migration result, for logging what moved and what was deliberately left behind.</summary>
public sealed record MigrationResult(TagKey FromKey, TagKey ToKey, MigrationOutcome Outcome, string? Value);

/// <summary>The outcome of applying one <see cref="FrameMigration"/>.</summary>
public enum MigrationOutcome
{
    /// <summary>Value copied to the target and removed from the source.</summary>
    Moved,

    /// <summary>Source key was empty/absent — nothing to do.</summary>
    SkippedEmpty,

    /// <summary>Target key already had a value — left untouched (idempotent re-run).</summary>
    SkippedTargetPresent,

    /// <summary>An <see cref="FrameMigration.Accept"/> guard rejected the value — left in the source key.</summary>
    SkippedRejectedValue,
}
