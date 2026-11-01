using System.Collections;
using SocialDanceTaggingStandard.Taxonomy;
using SocialDanceTaggingStandard.TestKit;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>
/// Drift-gate for the compiled baseline. <see cref="SocialDanceVocabulary.LoadDefault()"/> now returns
/// generated C# (<c>BaselineVocabulary.g.cs</c>); this asserts it is value-for-value identical to PARSING
/// the on-disk <c>data/vocabulary/</c> YAML (via the internal <c>LoadFromYamlDirectory</c> oracle) — so the
/// compiled baseline can only diverge from its source of truth through a generator bug, which fails here.
/// </summary>
public sealed class BaselineVocabularyDriftTests
{
    [Fact]
    public void GeneratedBaseline_MatchesYamlOnDisk()
    {
        var generated = SocialDanceVocabulary.LoadDefault();
        var fromYaml = SocialDanceVocabulary.LoadFromYamlDirectory(new DirectoryInfo(ConformancePaths.Resolve("data/vocabulary")));

        Assert.Equal(fromYaml.Version, generated.Version);

        Assert.Equal(
            fromYaml.Core.Keys.Select(k => k.Value).OrderBy(v => v, StringComparer.Ordinal),
            generated.Core.Keys.Select(k => k.Value).OrderBy(v => v, StringComparer.Ordinal));
        foreach (var key in fromYaml.Core.Keys)
            AssertDimensionEqual(fromYaml.Core[key], generated.Core[key]);

        Assert.Equal(
            fromYaml.Packs.Keys.Select(k => k.Value).OrderBy(v => v, StringComparer.Ordinal),
            generated.Packs.Keys.Select(k => k.Value).OrderBy(v => v, StringComparer.Ordinal));
        foreach (var key in fromYaml.Packs.Keys)
        {
            var expected = fromYaml.Packs[key];
            var actual = generated.Packs[key];
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.DisplayName, actual.DisplayName);
            Assert.Equal(expected.Description, actual.Description);
            Assert.Equal(expected.Families, actual.Families);
            Assert.Equal(expected.ReviewStatus, actual.ReviewStatus);
            AssertDimensionEqual(expected.Dances, actual.Dances);
        }
    }

    private static void AssertDimensionEqual(Dimension expected, Dimension actual)
    {
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Layer, actual.Layer);
        Assert.Equal(
            expected.AttributeReview.OrderBy(kv => kv.Key, StringComparer.Ordinal),
            actual.AttributeReview.OrderBy(kv => kv.Key, StringComparer.Ordinal));
        Assert.Equal(expected.Terms.Count, actual.Terms.Count);
        for (var i = 0; i < expected.Terms.Count; i++)
            AssertTermEqual(expected.Terms[i], actual.Terms[i]);
    }

    private static void AssertTermEqual(VocabularyTerm expected, VocabularyTerm actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Canonical, actual.Canonical);
        Assert.Equal(expected.Aliases, actual.Aliases);
        Assert.Equal(expected.Deprecated, actual.Deprecated);

        Assert.Equal(expected.Display?.Code, actual.Display?.Code);
        Assert.Equal(expected.Display?.Emoji, actual.Display?.Emoji);
        Assert.Equal(expected.Display?.Icon, actual.Display?.Icon);
        Assert.Equal(expected.Display?.Color, actual.Display?.Color);
        Assert.Equal(expected.Display?.ShortLabel, actual.Display?.ShortLabel);

        Assert.Equal(
            expected.Attributes.Keys.OrderBy(k => k, StringComparer.Ordinal),
            actual.Attributes.Keys.OrderBy(k => k, StringComparer.Ordinal));
        foreach (var k in expected.Attributes.Keys)
            Assert.True(
                DeepEqual(expected.Attributes[k], actual.Attributes[k]),
                $"attribute '{k}' on term '{expected.Id}' differs");
    }

    // The on-disk side (YamlDotNet) yields List&lt;object&gt; / Dictionary&lt;object,object&gt;; the generated
    // side yields object?[] / Dictionary&lt;string,object?&gt;. Compare by SHAPE + string content, not CLR type.
    private static bool DeepEqual(object? a, object? b)
    {
        if (a is null || b is null)
            return a is null && b is null;
        if (a is string || b is string)
            return Convert.ToString(a) == Convert.ToString(b);
        if (a is IDictionary da && b is IDictionary db)
        {
            if (da.Count != db.Count)
                return false;
            var bByKey = db.Keys.Cast<object>().ToDictionary(k => Convert.ToString(k) ?? "", k => db[k]);
            foreach (var key in da.Keys)
            {
                var ks = Convert.ToString(key) ?? "";
                if (!bByKey.TryGetValue(ks, out var bv) || !DeepEqual(da[key], bv))
                    return false;
            }
            return true;
        }
        if (a is IEnumerable ea && b is IEnumerable eb)
        {
            var la = ea.Cast<object?>().ToList();
            var lb = eb.Cast<object?>().ToList();
            return la.Count == lb.Count && la.Zip(lb, DeepEqual).All(eq => eq);
        }
        return Convert.ToString(a) == Convert.ToString(b);
    }
}
