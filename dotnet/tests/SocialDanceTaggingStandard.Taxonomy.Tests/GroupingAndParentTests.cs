using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class GroupingAndParentTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();

    private static readonly PackId[] AllPacks = [.. Vocab.Packs.Keys];

    [Fact]
    public void Dances_NormalizeDropsSwing_WhenASwingDanceIsPresent()
    {
        var result = Vocab.Dances(PackIds.Swing).Normalize(["Swing", "Lindy Hop"]);

        Assert.Equal(["Lindy Hop"], result.Select(t => t.Canonical));
    }

    [Fact]
    public void Dances_KeepSwing_WhenAloneOrWithAnotherFamilysDance()
    {
        var dances = Vocab.Dances(AllPacks);

        Assert.Equal(["Swing"], dances.Normalize(["Swing"]).Select(t => t.Canonical));
        Assert.Equal(2, dances.Normalize(["Swing", "Slow Waltz"]).Count);
    }

    [Fact]
    public void ChildrenOf_ListsTheDirectChildrenInCatalogOrder()
    {
        var dances = Vocab.Dances(PackIds.Swing);
        var swing = dances.Resolve("Swing")!;

        var children = dances.ChildrenOf(swing);

        Assert.NotEmpty(children);
        Assert.Contains(children, t => t.Canonical == "Lindy Hop");
        Assert.DoesNotContain(swing, children);
    }

    [Fact]
    public void EveryDanceParent_ResolvesWithinTheMergedDimension()
    {
        var dances = Vocab.Dances(AllPacks);

        foreach (var term in dances.Terms)
        {
            if (term.Attributes.GetValueOrDefault("parent") is string parent)
            {
                Assert.True(dances.Terms.Any(t => t.Id == parent), $"{term.Id} names unknown parent '{parent}'.");
            }
        }
    }

    [Fact]
    public void EarlyPack_CarriesOneStepPolkaAndFoxTrot()
    {
        var early = Vocab.Packs[PackIds.Early].Dances;

        Assert.Equal("One-Step", early.Resolve("One Step")!.Canonical);
        Assert.Equal("Polka", early.Resolve("Polka")!.Canonical);
        Assert.Equal("Fox Trot", early.Resolve("Foxtrot")!.Canonical);
    }

    [Fact]
    public void PackDances_GroupByFamily_WithALabelPerFamily()
    {
        var swing = Vocab.Packs[PackIds.Swing].Dances;
        var lindy = swing.Resolve("Lindy Hop")!;

        Assert.Equal("family", swing.Grouping!.Attribute);
        Assert.Equal("Swing partner", swing.Grouping.GroupOf(lindy)!.Label);
        Assert.Null(swing.Grouping.GroupOf(swing.Resolve("Swing")!));
    }

    [Fact]
    public void MergedDances_UnionThePacksGroupsWithoutDuplicates()
    {
        var merged = Vocab.Dances(AllPacks).Grouping!;
        var keys = merged.Groups.Select(g => g.Key).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
        Assert.Equal(Vocab.Packs.Values.SelectMany(p => p.Dances.Grouping!.Groups.Select(g => g.Key)).Distinct().Count(), keys.Count);
    }

    [Fact]
    public void CoreGroupings_NameTheirAttributeAndLabelEveryGroup()
    {
        var expected = new Dictionary<DimensionId, string>
        {
            [DimensionIds.Genre] = "family",
            [DimensionIds.RhythmicFeel] = "cluster",
            [DimensionIds.Instrumentation] = "size",
            [DimensionIds.Theme] = "category",
        };

        foreach (var (id, attribute) in expected)
        {
            var grouping = Vocab.Dimension(id)!.Grouping;

            Assert.NotNull(grouping);
            Assert.Equal(attribute, grouping.Attribute);
            Assert.All(grouping.Groups, g => Assert.False(string.IsNullOrWhiteSpace(g.Label)));
        }
    }

    [Fact]
    public void CoreGroupings_FileEveryTermUnderADeclaredGroup_WhereTheDimensionHasOne()
    {
        foreach (var id in new[] { DimensionIds.Genre, DimensionIds.RhythmicFeel, DimensionIds.Instrumentation, DimensionIds.Theme })
        {
            var dimension = Vocab.Dimension(id)!;

            Assert.All(dimension.Terms, t => Assert.NotNull(dimension.Grouping!.GroupOf(t)));
        }
    }

    [Fact]
    public void FlatDimensions_HaveNoGrouping()
    {
        Assert.Null(Vocab.Dimension(DimensionIds.Mood)!.Grouping);
        Assert.Null(Vocab.Dimension(DimensionIds.Instrument)!.Grouping);
    }

    [Fact]
    public void TermGrouping_RejectsEmptyAndDuplicateGroups()
    {
        Assert.Throws<SdtsVocabularyException>(() => new TermGrouping("family", []));
        Assert.Throws<SdtsVocabularyException>(() => new TermGrouping("family", [new("a", "A"), new("a", "Again")]));
    }

    [Fact]
    public void DanceColours_AreUniqueAcrossThePacks()
    {
        var colours = Vocab.Dances(AllPacks).Terms
            .Select(t => t.Display?.Color)
            .Where(c => c is not null)
            .ToList();

        Assert.Equal(colours.Count, colours.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }
}
