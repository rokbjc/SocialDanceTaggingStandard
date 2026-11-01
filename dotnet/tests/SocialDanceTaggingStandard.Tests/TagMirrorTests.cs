using SocialDanceTaggingStandard.Sync;
using SocialDanceTaggingStandard.TestKit;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class TagMirrorTests
{
    private const string CanonKey = "C";
    private const string MirrorKey = "M";
    private const string StateKey = "state";

    private static readonly IReadOnlyList<FieldMirror> Map =
        [new FieldMirror(new FieldId("f"), new TagKey(CanonKey), new TagKey(MirrorKey))];

    private static TagMirror Build(
        InMemoryTagSource canonical, InMemoryTagSource mirror, InMemoryTagSource state,
        MirrorSyncOptions? options = null) =>
        new(canonical, mirror, state, StateKey, Map, options);

    /// <summary>Returns three sources already synced on <paramref name="value"/> (baseline recorded).</summary>
    private static (InMemoryTagSource Canon, InMemoryTagSource Mirror, InMemoryTagSource State) Synced(string value)
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();
        canon.Set(new TagKey(CanonKey), value);
        Build(canon, mirror, state).Sync();
        return (canon, mirror, state);
    }

    // ---- first sync (no baseline) ----

    [Fact]
    public void FirstSync_CanonicalOnly_Exports()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();
        canon.Set(new TagKey(CanonKey), "X");

        SyncReport report = Build(canon, mirror, state).Sync();

        Assert.Equal("X", mirror.Get(new TagKey(MirrorKey)));
        Assert.Equal(FieldSyncOutcome.Exported, report.Fields[0].Outcome);
    }

    [Fact]
    public void FirstSync_MirrorOnly_Imports()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();
        mirror.Set(new TagKey(MirrorKey), "Y");

        SyncReport report = Build(canon, mirror, state).Sync();

        Assert.Equal("Y", canon.Get(new TagKey(CanonKey)));
        Assert.Equal(FieldSyncOutcome.Imported, report.Fields[0].Outcome);
    }

    [Fact]
    public void FirstSync_BothPresentEqual_Unchanged()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();
        canon.Set(new TagKey(CanonKey), "Same");
        mirror.Set(new TagKey(MirrorKey), "Same");

        SyncReport report = Build(canon, mirror, state).Sync();

        Assert.Equal(FieldSyncOutcome.Unchanged, report.Fields[0].Outcome);
        Assert.Equal(0, report.Changed);
    }

    [Fact]
    public void FirstSync_BothPresentDiffer_IsConflict()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();
        canon.Set(new TagKey(CanonKey), "A");
        mirror.Set(new TagKey(MirrorKey), "B");

        SyncReport report = Build(canon, mirror, state).Sync();

        Assert.Equal(FieldSyncOutcome.Conflict, report.Fields[0].Outcome);
        Assert.True(report.HasUnresolvedConflicts);
        Assert.Equal("A", canon.Get(new TagKey(CanonKey)));
        Assert.Equal("B", mirror.Get(new TagKey(MirrorKey)));
    }

    [Fact]
    public void BothAbsent_Unchanged()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();

        SyncReport report = Build(canon, mirror, state).Sync();

        Assert.Equal(FieldSyncOutcome.Unchanged, report.Fields[0].Outcome);
    }

    // ---- incremental edits after a baseline ----

    [Fact]
    public void MirrorEditOnly_Imports()
    {
        var (canon, mirror, state) = Synced("X");
        mirror.Set(new TagKey(MirrorKey), "edited-in-mm5");

        SyncReport report = Build(canon, mirror, state).Sync();

        Assert.Equal("edited-in-mm5", canon.Get(new TagKey(CanonKey)));
        Assert.Equal(FieldSyncOutcome.Imported, report.Fields[0].Outcome);
    }

    [Fact]
    public void CanonicalEditOnly_Exports()
    {
        var (canon, mirror, state) = Synced("X");
        canon.Set(new TagKey(CanonKey), "edited-in-editor");

        SyncReport report = Build(canon, mirror, state).Sync();

        Assert.Equal("edited-in-editor", mirror.Get(new TagKey(MirrorKey)));
        Assert.Equal(FieldSyncOutcome.Exported, report.Fields[0].Outcome);
    }

    [Fact]
    public void CanonicalRemoval_PropagatesToMirror()
    {
        var (canon, mirror, state) = Synced("X");
        canon.Set(new TagKey(CanonKey), null);

        SyncReport report = Build(canon, mirror, state).Sync();

        Assert.Null(mirror.Get(new TagKey(MirrorKey)));
        Assert.Equal(FieldSyncOutcome.Exported, report.Fields[0].Outcome);
    }

    // ---- conflicts ----

    [Fact]
    public void BothEdited_ReportOnly_DoesNotClobber()
    {
        var (canon, mirror, state) = Synced("base");
        canon.Set(new TagKey(CanonKey), "A");
        mirror.Set(new TagKey(MirrorKey), "B");

        SyncReport report = Build(canon, mirror, state).Sync();

        Assert.Equal(FieldSyncOutcome.Conflict, report.Fields[0].Outcome);
        Assert.False(report.Fields[0].Resolved);
        Assert.Equal("A", canon.Get(new TagKey(CanonKey)));
        Assert.Equal("B", mirror.Get(new TagKey(MirrorKey)));
    }

    [Fact]
    public void ReportOnlyConflict_ResurfacesOnNextSync()
    {
        var (canon, mirror, state) = Synced("base");
        canon.Set(new TagKey(CanonKey), "A");
        mirror.Set(new TagKey(MirrorKey), "B");

        Build(canon, mirror, state).Sync();
        SyncReport second = Build(canon, mirror, state).Sync();

        Assert.Equal(FieldSyncOutcome.Conflict, second.Fields[0].Outcome);
    }

    [Fact]
    public void BothEdited_PreferCanonical_OverwritesMirror()
    {
        var (canon, mirror, state) = Synced("base");
        canon.Set(new TagKey(CanonKey), "A");
        mirror.Set(new TagKey(MirrorKey), "B");

        SyncReport report = Build(canon, mirror, state,
            new MirrorSyncOptions { OnConflict = ConflictResolution.PreferCanonical }).Sync();

        Assert.Equal("A", mirror.Get(new TagKey(MirrorKey)));
        Assert.True(report.Fields[0].Resolved);
    }

    [Fact]
    public void BothEdited_PreferMirror_OverwritesCanonical()
    {
        var (canon, mirror, state) = Synced("base");
        canon.Set(new TagKey(CanonKey), "A");
        mirror.Set(new TagKey(MirrorKey), "B");

        SyncReport report = Build(canon, mirror, state,
            new MirrorSyncOptions { OnConflict = ConflictResolution.PreferMirror }).Sync();

        Assert.Equal("B", canon.Get(new TagKey(CanonKey)));
        Assert.True(report.Fields[0].Resolved);
    }

    // ---- direction policies ----

    [Fact]
    public void ImportOnly_SkipsCanonicalExport()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();
        canon.Set(new TagKey(CanonKey), "P");

        SyncReport report = Build(canon, mirror, state,
            new MirrorSyncOptions { Direction = MirrorDirection.ImportOnly }).Sync();

        Assert.Null(mirror.Get(new TagKey(MirrorKey)));
        Assert.Equal(FieldSyncOutcome.Skipped, report.Fields[0].Outcome);
    }

    [Fact]
    public void ExportOnly_IgnoresMirrorEdit()
    {
        var (canon, mirror, state) = Synced("X");
        mirror.Set(new TagKey(MirrorKey), "mm5-change");

        SyncReport report = Build(canon, mirror, state,
            new MirrorSyncOptions { Direction = MirrorDirection.ExportOnly }).Sync();

        Assert.Equal("X", canon.Get(new TagKey(CanonKey)));
        Assert.Equal(FieldSyncOutcome.Skipped, report.Fields[0].Outcome);
    }

    [Fact]
    public void NoneDirection_WritesNothing()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();
        canon.Set(new TagKey(CanonKey), "P");

        SyncReport report = Build(canon, mirror, state,
            new MirrorSyncOptions { Direction = MirrorDirection.None }).Sync();

        Assert.Null(mirror.Get(new TagKey(MirrorKey)));
        Assert.Equal(FieldSyncOutcome.Skipped, report.Fields[0].Outcome);
    }

    // ---- WriteCanonical / write-through ----

    [Fact]
    public void WriteCanonical_WriteThrough_UpdatesMirrorImmediately()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();

        Build(canon, mirror, state).WriteCanonical("f", "Q");

        Assert.Equal("Q", canon.Get(new TagKey(CanonKey)));
        Assert.Equal("Q", mirror.Get(new TagKey(MirrorKey)));
    }

    [Fact]
    public void WriteCanonical_NoWriteThrough_DefersMirror()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();

        Build(canon, mirror, state, new MirrorSyncOptions { WriteThrough = false })
            .WriteCanonical("f", "Q");

        Assert.Equal("Q", canon.Get(new TagKey(CanonKey)));
        Assert.Null(mirror.Get(new TagKey(MirrorKey)));
    }

    [Fact]
    public void WriteCanonical_ImportOnly_DoesNotWriteMirror()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();

        Build(canon, mirror, state, new MirrorSyncOptions { Direction = MirrorDirection.ImportOnly })
            .WriteCanonical("f", "Q");

        Assert.Null(mirror.Get(new TagKey(MirrorKey)));
    }

    [Fact]
    public void WriteCanonical_UnmappedField_Throws()
    {
        var canon = new InMemoryTagSource();
        var mirror = new InMemoryTagSource();
        var state = new InMemoryTagSource();

        Assert.Throws<ArgumentException>(() => Build(canon, mirror, state).WriteCanonical("nope", "x"));
    }

    // ---- one-way operations ----

    [Fact]
    public void Import_ForcesMirrorIntoCanonical()
    {
        var (canon, mirror, state) = Synced("X");
        mirror.Set(new TagKey(MirrorKey), "fromMirror");

        SyncReport report = Build(canon, mirror, state).Import();

        Assert.Equal("fromMirror", canon.Get(new TagKey(CanonKey)));
        Assert.Equal(FieldSyncOutcome.Imported, report.Fields[0].Outcome);
    }

    [Fact]
    public void Export_ForcesCanonicalIntoMirror()
    {
        var (canon, mirror, state) = Synced("X");
        canon.Set(new TagKey(CanonKey), "fromCanon");

        SyncReport report = Build(canon, mirror, state).Export();

        Assert.Equal("fromCanon", mirror.Get(new TagKey(MirrorKey)));
        Assert.Equal(FieldSyncOutcome.Exported, report.Fields[0].Outcome);
    }

    // ---- baseline persistence ----

    [Fact]
    public void Baseline_PreventsReExportOfUnchangedFields()
    {
        var (canon, mirror, state) = Synced("X");

        SyncReport report = Build(canon, mirror, state).Sync();

        Assert.Equal(FieldSyncOutcome.Unchanged, report.Fields[0].Outcome);
    }
}
