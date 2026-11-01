using System.Text.Json;
using SocialDanceTaggingStandard.Internal;
using SocialDanceTaggingStandard.Taxonomy;
using SocialDanceTaggingStandard.Taxonomy.Profile;
using SocialDanceTaggingStandard.TestKit;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>
/// Runs the language-neutral Personal Profile resolution vectors under
/// <c>conformance/vectors/profile/resolution.json</c> — the one normative obligation of the optional
/// profile layer (profile.md §9): alias / cross-dimension projection, hash-safety, and import governance.
/// </summary>
public sealed class ProfileResolutionConformanceTests
{
    private static readonly SocialDanceVocabulary Baseline = SocialDanceVocabulary.LoadDefault();

    public static IEnumerable<object?[]> Cases()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(
            ConformancePaths.Resolve("conformance/vectors/profile/resolution.json")));
        foreach (var c in doc.RootElement.GetProperty("cases").EnumerateArray())
            yield return [c.GetProperty("name").GetString(), c.GetRawText()];
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Profile_MatchesVectors(string name, string json)
    {
        _ = name;
        using var doc = JsonDocument.Parse(json);
        var c = doc.RootElement;
        var profile = BuildProfile(c.GetProperty("profile"));
        var expect = c.GetProperty("expect");

        switch (c.GetProperty("kind").GetString())
        {
            case "project":
                {
                    var result = Baseline.WithProfile(profile).Mapper.Project(ParseTags(c.GetProperty("input")));
                    var projected = expect.GetProperty("projected");
                    var expectedKeys = projected.EnumerateObject().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    Assert.Equal(expectedKeys.Count, result.Projected.Count);   // no extra/dropped dimensions
                    foreach (var dim in projected.EnumerateObject())
                    {
                        Assert.True(result.Projected.ContainsKey(dim.Name), $"missing projected dimension '{dim.Name}'");
                        Assert.Equal(StrArray(dim.Value), result.Projected[dim.Name]);
                    }
                    break;
                }
            case "validate":
                {
                    var report = ProfileValidator.Validate(Baseline, profile);
                    if (expect.TryGetProperty("valid", out var valid))
                        Assert.Equal(valid.GetBoolean(), report.IsValid);
                    if (expect.TryGetProperty("errors_contain", out var errs))
                        foreach (var e in errs.EnumerateArray())
                            Assert.Contains(report.Errors, x => x.Contains(e.GetString()!));
                    if (expect.TryGetProperty("warnings_contain", out var warns))
                        foreach (var w in warns.EnumerateArray())
                            Assert.Contains(report.Warnings, x => x.Contains(w.GetString()!));
                    break;
                }
            case "hash":
                {
                    var vocab = Baseline.WithProfile(profile).Vocabulary;
                    var t = expect.GetProperty("term");
                    var dim = DimFor(vocab, t.GetProperty("dimension").GetString()!);
                    var term = dim!.Resolve(t.GetProperty("value").GetString()!);
                    Assert.NotNull(term);
                    Assert.Equal(StableHash.Of(expect.GetProperty("hash_of").GetString()), StableHash.Of(term!.Canonical));
                    break;
                }
            case "migrate":
                {
                    var plan = Baseline.WithProfile(profile).Mapper.PlanMigration(ParseTags(c.GetProperty("input")));
                    AssertWrites(expect.GetProperty("writes"), plan.Writes);
                    AssertWrites(expect.GetProperty("removals"), plan.Removals);
                    break;
                }
            default:
                throw new Xunit.Sdk.XunitException("unknown vector kind");
        }
    }

    private static void AssertWrites(JsonElement expected, IReadOnlyList<FieldWrite> actual)
    {
        var exp = expected.EnumerateArray()
            .Select(w => (w.GetProperty("dimension").GetString()!, w.GetProperty("value").GetString()!))
            .ToHashSet();
        var act = actual.Select(w => (w.Dimension, w.Value)).ToHashSet();
        Assert.True(exp.SetEquals(act), $"expected [{string.Join(", ", exp)}] but got [{string.Join(", ", act)}]");
    }

    // ── vector → domain helpers ─────────────────────────────────────────────────

    private static PersonalProfile BuildProfile(JsonElement spec)
    {
        var overlay = new VocabularyOverlay();

        if (spec.TryGetProperty("add_terms", out var addTerms))
            foreach (var dim in addTerms.EnumerateObject())
                foreach (var t in dim.Value.EnumerateArray())
                    overlay.AddTerm(new DimensionId(dim.Name), new VocabularyTerm(
                        t.GetProperty("id").GetString()!,
                        t.GetProperty("canonical").GetString()!,
                        t.TryGetProperty("aliases", out var al) ? StrArray(al) : [],
                        false, new Dictionary<string, object?>()));

        if (spec.TryGetProperty("add_aliases", out var addAliases))
            foreach (var dim in addAliases.EnumerateObject())
                foreach (var term in dim.Value.EnumerateObject())
                    foreach (var alias in term.Value.EnumerateArray())
                        overlay.AddAlias(new DimensionId(dim.Name), term.Name, alias.GetString()!);

        // same-dimension mapping.alias rules fold into add_aliases (one resolution mechanism)
        if (spec.TryGetProperty("alias", out var aliasRules))
            foreach (var r in aliasRules.EnumerateArray())
                overlay.AddAlias(new DimensionId(r.GetProperty("from_dimension").GetString()!), r.GetProperty("to_value").GetString()!,
                    r.GetProperty("from_value").GetString()!);

        var colors = new List<ColorOverride>();
        if (spec.TryGetProperty("display", out var display))
            foreach (var d in display.EnumerateArray())
                colors.Add(new ColorOverride(
                    d.GetProperty("dimension").GetString()!,
                    d.GetProperty("value").GetString()!,
                    d.TryGetProperty("color", out var col) ? col.GetString() : null,
                    d.TryGetProperty("label", out var lab) ? lab.GetString() : null));

        var cross = new List<CrossDimensionRule>();
        if (spec.TryGetProperty("cross_dimension", out var crossRules))
            foreach (var r in crossRules.EnumerateArray())
                cross.Add(new CrossDimensionRule(
                    r.GetProperty("from_dimension").GetString()!, r.GetProperty("from_value").GetString()!,
                    r.GetProperty("to_dimension").GetString()!, r.GetProperty("to_value").GetString()!,
                    KeepSource: !(r.TryGetProperty("on_source", out var os) && os.GetString() == "drop"),
                    Migrate: r.TryGetProperty("migrate", out var mg) && mg.GetBoolean(),
                    Match: r.TryGetProperty("match", out var mt) && mt.GetString() == "exact"
                        ? MatchMode.Exact : MatchMode.CaseInsensitive));

        return new PersonalProfile(overlay, colors, cross);
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ParseTags(JsonElement input)
        => input.EnumerateObject().ToDictionary(
            p => p.Name, p => (IReadOnlyList<string>)StrArray(p.Value), StringComparer.OrdinalIgnoreCase);

    private static string[] StrArray(JsonElement arr)
        => [.. arr.EnumerateArray().Select(e => e.GetString()!)];

    private static Dimension? DimFor(SocialDanceVocabulary vocab, string field)
        => string.Equals(field, "dances", StringComparison.OrdinalIgnoreCase)
            ? (vocab.Packs.Count > 0 ? vocab.Dances([.. vocab.Packs.Keys]) : null)
            : vocab.Dimension(new DimensionId(field));
}
