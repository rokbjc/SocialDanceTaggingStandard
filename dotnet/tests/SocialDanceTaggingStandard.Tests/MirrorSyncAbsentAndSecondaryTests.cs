using SocialDanceTaggingStandard.Sync;
using SocialDanceTaggingStandard.TestKit;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>The two mirror-sync policies added for the SP2 convergence gaps: an absent mirror is unknown
/// vs cleared (never import absence as a deletion), and secondary mirror keys are realigned vs left alone
/// (never flatten or mint a distinct legacy secondary). Both default to the pre-existing behaviour.</summary>
public sealed class MirrorSyncAbsentAndSecondaryTests
{
    private static readonly IReadOnlyList<FieldMirror> SingleMap =
        [new FieldMirror(new FieldId("dances"), new TagKey("C"), new TagKey("M1"))];

    private static readonly IReadOnlyList<FieldMirror> DualMap =
        [new FieldMirror(new FieldId("dances"), new TagKey("C"), [new TagKey("M1"), new TagKey("M2")])];

    // Syncs a single-key mapping to a live baseline (both sides = value), then returns the pieces so a test
    // can make the mirror vanish and reconcile again.
    private static (InMemoryTagSource Canon, InMemoryTagSource Mirror, SyncState State, MirrorSyncEngine Engine) SyncedSingle(string value)
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new SyncState();
        var engine = new MirrorSyncEngine();
        canon.Set(new TagKey("C"), value);
        engine.Reconcile(canon, mirror, state, SingleMap, MirrorSyncOptions.Default);
        return (canon, mirror, state, engine);
    }

    [Fact]
    public void AbsentMirror_Cleared_ImportsAbsenceAsDeletion_TheDefault()
    {
        var (canon, mirror, state, engine) = SyncedSingle("Lindy Hop");
        mirror.Set(new TagKey("M1"), null); // the mirror frame vanishes

        var report = engine.Reconcile(canon, mirror, state, SingleMap, MirrorSyncOptions.Default);

        Assert.Null(canon.Get(new TagKey("C")));                       // canonical deleted — the reported loss
        Assert.Equal(FieldSyncOutcome.Imported, report.Fields[0].Outcome);
    }

    [Fact]
    public void AbsentMirror_Unknown_KeepsCanonicalAndRematerialisesTheMirror()
    {
        var (canon, mirror, state, engine) = SyncedSingle("Lindy Hop");
        mirror.Set(new TagKey("M1"), null);

        var options = MirrorSyncOptions.Default with { AbsentMirror = AbsentMirrorPolicy.Unknown };
        engine.Reconcile(canon, mirror, state, SingleMap, options);

        Assert.Equal("Lindy Hop", canon.Get(new TagKey("C")));         // canonical stands, absence is not a deletion
        Assert.Equal("Lindy Hop", mirror.Get(new TagKey("M1")));       // the vanished mirror heals from canonical
    }

    [Fact]
    public void AbsentMirror_Unknown_ImportOnly_NeverDeletesCanonicalEvenWhenExportIsForbidden()
    {
        var (canon, mirror, state, engine) = SyncedSingle("Lindy Hop");
        mirror.Set(new TagKey("M1"), null);

        var options = MirrorSyncOptions.Default with
        {
            AbsentMirror = AbsentMirrorPolicy.Unknown,
            Direction = MirrorDirection.ImportOnly,
        };
        engine.Reconcile(canon, mirror, state, SingleMap, options);

        Assert.Equal("Lindy Hop", canon.Get(new TagKey("C")));
    }

    [Fact]
    public void Export_SecondaryLeave_DoesNotFlattenADistinctLegacySecondary()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new SyncState();
        canon.Set(new TagKey("C"), "Lindy Hop");
        mirror.Set(new TagKey("M2"), "#BLB; #SLB"); // a distinct legacy secondary frame (e.g. MusicMatch_Situation)

        var options = MirrorSyncOptions.Default with { SecondaryMirror = SecondaryMirrorPolicy.Leave };
        new MirrorSyncEngine().Reconcile(canon, mirror, state, DualMap, options);

        Assert.Equal("Lindy Hop", mirror.Get(new TagKey("M1")));       // primary written
        Assert.Equal("#BLB; #SLB", mirror.Get(new TagKey("M2")));      // legacy secondary untouched
    }

    [Fact]
    public void Export_SecondaryRealign_FlattensTheSecondary_TheDefault()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new SyncState();
        canon.Set(new TagKey("C"), "Lindy Hop");
        mirror.Set(new TagKey("M2"), "#BLB; #SLB");

        new MirrorSyncEngine().Reconcile(canon, mirror, state, DualMap, MirrorSyncOptions.Default);

        Assert.Equal("Lindy Hop", mirror.Get(new TagKey("M2")));       // documents the default that lost the legacy value
    }

    [Fact]
    public void Export_SecondaryLeave_DoesNotMintAnAbsentSecondary()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new SyncState();
        canon.Set(new TagKey("C"), "Lindy Hop"); // M2 never existed on this file

        var options = MirrorSyncOptions.Default with { SecondaryMirror = SecondaryMirrorPolicy.Leave };
        new MirrorSyncEngine().Reconcile(canon, mirror, state, DualMap, options);

        Assert.Null(mirror.Get(new TagKey("M2")));                     // not minted onto a file that never carried it
    }

    [Fact]
    public void Import_SecondaryLeave_DoesNotMintTheSecondaryFromThePrimary()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new SyncState();
        mirror.Set(new TagKey("M1"), "Balboa"); // primary only

        var options = MirrorSyncOptions.Default with { SecondaryMirror = SecondaryMirrorPolicy.Leave };
        new MirrorSyncEngine().Reconcile(canon, mirror, state, DualMap, options);

        Assert.Equal("Balboa", canon.Get(new TagKey("C")));            // imported from the primary
        Assert.Null(mirror.Get(new TagKey("M2")));                     // secondary left absent (the default would mint it)
    }
}
