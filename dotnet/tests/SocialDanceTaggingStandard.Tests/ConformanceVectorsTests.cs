using System.Text.Json;
using SocialDanceTaggingStandard.Internal;
using SocialDanceTaggingStandard.Sync;
using SocialDanceTaggingStandard.TestKit;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>
/// Runs the shared language-neutral vectors under <c>conformance/vectors/</c> — the SAME files the
/// Python suite consumes — so the two implementations can't silently diverge.
/// </summary>
public sealed class ConformanceVectorsTests
{
    private static string? Str(JsonElement parent, string prop)
    {
        JsonElement e = parent.GetProperty(prop);
        return e.ValueKind == JsonValueKind.Null ? null : e.GetString();
    }

    public static IEnumerable<object?[]> HashCases()
    {
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(ConformancePaths.VectorsDir, "hash.json")));
        foreach (JsonElement c in doc.RootElement.GetProperty("cases").EnumerateArray())
        {
            yield return [Str(c, "input"), c.GetProperty("hash").GetString()];
        }
    }

    [Theory]
    [MemberData(nameof(HashCases))]
    public void StableHash_MatchesVectors(string? input, string expected) =>
        Assert.Equal(expected, StableHash.Of(input));

    public static IEnumerable<object?[]> ReconcileCases()
    {
        using JsonDocument doc = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(ConformancePaths.VectorsDir, "sync", "reconcile.json")));
        JsonElement root = doc.RootElement;
        string field = root.GetProperty("field").GetString()!;
        string ck = root.GetProperty("canonical_key").GetString()!;
        string mk = root.GetProperty("mirror_key").GetString()!;

        foreach (JsonElement c in root.GetProperty("cases").EnumerateArray())
        {
            JsonElement o = c.GetProperty("options");
            JsonElement ex = c.GetProperty("expect");
            yield return
            [
                c.GetProperty("name").GetString(),
                c.TryGetProperty("op", out JsonElement op) ? op.GetString()! : "sync",
                field, ck, mk,
                Str(c, "canonical_in"), Str(c, "mirror_in"), Str(c, "baseline_value"),
                o.GetProperty("direction").GetString()!,
                o.GetProperty("on_conflict").GetString()!,
                o.GetProperty("write_through").GetBoolean(),
                ex.GetProperty("outcome").GetString()!,
                Str(ex, "canonical_out"), Str(ex, "mirror_out"),
                ex.GetProperty("resolved").GetBoolean()
            ];
        }
    }

    [Theory]
    [MemberData(nameof(ReconcileCases))]
    public void Reconcile_MatchesVectors(
        string name, string op, string field, string ck, string mk,
        string? canonicalIn, string? mirrorIn, string? baselineValue,
        string direction, string onConflict, bool writeThrough,
        string expectedOutcome, string? canonicalOut, string? mirrorOut, bool resolved)
    {
        _ = name;

        var canonical = new InMemoryTagSource();
        if (canonicalIn is not null)
            canonical.Set(new TagKey(ck), canonicalIn);
        var mirror = new InMemoryTagSource();
        if (mirrorIn is not null)
            mirror.Set(new TagKey(mk), mirrorIn);

        var state = new InMemoryTagSource();
        if (baselineValue is not null)
        {
            var seeded = new SyncState();
            seeded.Set(new FieldId(field), StableHash.Of(baselineValue));
            state.Set(new TagKey("state"), seeded.Encode());
        }

        var map = new[] { new FieldMirror(new FieldId(field), new TagKey(ck), new TagKey(mk)) };
        var options = new MirrorSyncOptions
        {
            Direction = Enum.Parse<MirrorDirection>(direction),
            OnConflict = Enum.Parse<ConflictResolution>(onConflict),
            WriteThrough = writeThrough,
        };

        var tagMirror = new TagMirror(canonical, mirror, state, "state", map, options);
        SyncReport report = op switch
        {
            "import" => tagMirror.Import(),
            "export" => tagMirror.Export(),
            _ => tagMirror.Sync(),
        };

        FieldSyncResult r = report.Fields[0];
        Assert.Equal(expectedOutcome, r.Outcome.ToString());
        Assert.Equal(canonicalOut, canonical.Get(new TagKey(ck)));
        Assert.Equal(mirrorOut, mirror.Get(new TagKey(mk)));
        Assert.Equal(resolved, r.Resolved);
    }
}
