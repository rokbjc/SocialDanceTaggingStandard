using SocialDanceTaggingStandard.Sync;
using SocialDanceTaggingStandard.TestKit;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class MirrorSyncEngineMultiMirrorTests
{
    private static readonly IReadOnlyList<FieldMirror> DualMap =
        [new FieldMirror(new FieldId("dances"), new TagKey("C"), [new TagKey("M1"), new TagKey("M2")])];

    [Fact]
    public void Export_WritesValueToAllMirrorKeys()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new SyncState();
        canon.Set(new TagKey("C"), "Lindy Hop");

        new MirrorSyncEngine().Reconcile(canon, mirror, state, DualMap, MirrorSyncOptions.Default);

        Assert.Equal("Lindy Hop", mirror.Get(new TagKey("M1")));
        Assert.Equal("Lindy Hop", mirror.Get(new TagKey("M2")));
    }

    [Fact]
    public void Import_FromPrimary_AlignsSecondaryMirror()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new SyncState();
        mirror.Set(new TagKey("M1"), "Balboa"); // primary only — secondary stale/absent

        new MirrorSyncEngine().Reconcile(canon, mirror, state, DualMap, MirrorSyncOptions.Default);

        Assert.Equal("Balboa", canon.Get(new TagKey("C")));
        Assert.Equal("Balboa", mirror.Get(new TagKey("M2"))); // secondary realigned to primary
    }

    [Fact]
    public void ChangeDetection_UsesPrimaryMirrorOnly()
    {
        // Baseline both sides equal on "X".
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new SyncState();
        canon.Set(new TagKey("C"), "X");
        var engine = new MirrorSyncEngine();
        engine.Reconcile(canon, mirror, state, DualMap, MirrorSyncOptions.Default); // M1=M2=X, baseline set

        // Editing the canonical exports to all mirrors.
        canon.Set(new TagKey("C"), "Y");
        SyncReport report = engine.Reconcile(canon, mirror, state, DualMap, MirrorSyncOptions.Default);

        Assert.Equal(FieldSyncOutcome.Exported, report.Fields[0].Outcome);
        Assert.Equal("Y", mirror.Get(new TagKey("M1")));
        Assert.Equal("Y", mirror.Get(new TagKey("M2")));
    }
}
