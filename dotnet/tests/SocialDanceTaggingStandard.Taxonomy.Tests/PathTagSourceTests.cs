using SocialDanceTaggingStandard.Sync;
using SocialDanceTaggingStandard.Taxonomy;
using SocialDanceTaggingStandard.Taxonomy.Ingest;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class PathTagSourceTests
{
    private static SocialDanceVocabulary Vocab() => SocialDanceVocabulary.LoadDefault();

    private static PathIngest StojanIngest() => new(Vocab(),
    [
        new PathRule("swing/!Stojan/*.mp3", "%artist% - %title% <tail>",
            [new("LIN", "dances", "Lindy Hop"), new("BAL", "dances", "Balboa")],
            Rating: new PlusRunRating()),
    ]);

    [Fact]
    public void Serves_projected_fields_by_id_as_a_tag_source()
    {
        ISdtsSource source = PathTagSource.For(StojanIngest(), "swing/!Stojan/Bob Crosby - Ja-Da BAL LIN ++++++.mp3")!;

        Assert.Equal(["Balboa", "Lindy Hop"], source.Get(new TagKey("dances"))!.Split("; ").OrderBy(x => x));
        Assert.Equal("Bob Crosby", source.Get(new TagKey("artist")));
        Assert.Equal("6", source.Get(new TagKey("rating")));
        Assert.Null(source.Get(new TagKey("genre")));   // a field the path did not yield
    }

    [Fact]
    public void For_returns_null_when_no_rule_matches()
        => Assert.Null(PathTagSource.For(StojanIngest(), "blues/whatever.mp3"));

    [Fact]
    public void Set_throws_because_a_path_is_read_only()
    {
        ISdtsSource source = PathTagSource.For(StojanIngest(), "swing/!Stojan/Bob Crosby - Ja-Da BAL ++.mp3")!;
        Assert.Throws<NotSupportedException>(() => source.Set(new TagKey("dances"), "Charleston"));
    }
}
