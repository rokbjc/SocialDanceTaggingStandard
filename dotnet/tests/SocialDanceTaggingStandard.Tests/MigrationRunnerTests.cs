using SocialDanceTaggingStandard.Sync;
using SocialDanceTaggingStandard.TestKit;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class MigrationRunnerTests
{
    [Fact]
    public void Move_CopiesFromKeyToToKeyAndRemovesFrom()
    {
        var src = new InMemoryTagSource();
        src.Set(new TagKey("SB-Beats"), "[1,2,3]");

        IReadOnlyList<MigrationResult> results =
            MigrationRunner.Apply(src, [new FrameMigration(new TagKey("SB-Beats"), new TagKey("SG_Beats"))]);

        Assert.Equal("[1,2,3]", src.Get(new TagKey("SG_Beats")));
        Assert.Null(src.Get(new TagKey("SB-Beats")));
        Assert.Equal(MigrationOutcome.Moved, results[0].Outcome);
    }

    [Fact]
    public void Idempotent_SkipsWhenTargetAlreadyPresent()
    {
        var src = new InMemoryTagSource();
        src.Set(new TagKey("SG_Beats"), "already-here");
        src.Set(new TagKey("SB-Beats"), "stale"); // should be left untouched, not clobber the target

        IReadOnlyList<MigrationResult> results =
            MigrationRunner.Apply(src, [new FrameMigration(new TagKey("SB-Beats"), new TagKey("SG_Beats"))]);

        Assert.Equal("already-here", src.Get(new TagKey("SG_Beats")));
        Assert.Equal("stale", src.Get(new TagKey("SB-Beats")));
        Assert.Equal(MigrationOutcome.SkippedTargetPresent, results[0].Outcome);
    }

    [Fact]
    public void SkipsWhenSourceEmpty()
    {
        var src = new InMemoryTagSource();

        IReadOnlyList<MigrationResult> results =
            MigrationRunner.Apply(src, [new FrameMigration(new TagKey("SB-Beats"), new TagKey("SG_Beats"))]);

        Assert.Null(src.Get(new TagKey("SG_Beats")));
        Assert.Equal(MigrationOutcome.SkippedEmpty, results[0].Outcome);
    }

    [Fact]
    public void AcceptPredicate_RejectsValue_LeavesItInPlace()
    {
        var src = new InMemoryTagSource();
        src.Set(new TagKey("Quality"), "Good"); // not a recording-era enum member

        IReadOnlyList<MigrationResult> results = MigrationRunner.Apply(src, [
            new FrameMigration(new TagKey("Quality"), new TagKey("SDTS_RECORDING_QUALITY_ERA"),
                Accept: v => v is "Acoustic Era" or "Electrical Mono" or "Hi-Fi Mono" or "Stereo Era" or "Digital Era")
        ]);

        Assert.Null(src.Get(new TagKey("SDTS_RECORDING_QUALITY_ERA")));     // not imported
        Assert.Equal("Good", src.Get(new TagKey("Quality")));     // left in place
        Assert.Equal(MigrationOutcome.SkippedRejectedValue, results[0].Outcome);
    }

    [Fact]
    public void AcceptPredicate_AcceptsValidValue_Moves()
    {
        var src = new InMemoryTagSource();
        src.Set(new TagKey("Quality"), "Hi-Fi Mono");

        MigrationRunner.Apply(src, [
            new FrameMigration(new TagKey("Quality"), new TagKey("SDTS_RECORDING_QUALITY_ERA"),
                Accept: v => v is "Acoustic Era" or "Electrical Mono" or "Hi-Fi Mono" or "Stereo Era" or "Digital Era")
        ]);

        Assert.Equal("Hi-Fi Mono", src.Get(new TagKey("SDTS_RECORDING_QUALITY_ERA")));
        Assert.Null(src.Get(new TagKey("Quality")));
    }
}
