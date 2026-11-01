using System.Text.Json;
using SocialDanceTaggingStandard.Taxonomy;
using SocialDanceTaggingStandard.Taxonomy.Ingest;
using SocialDanceTaggingStandard.Taxonomy.Profile;
using SocialDanceTaggingStandard.TestKit;
using SocialDanceTaggingStandard.Validation;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class PathIngestTests
{
    private static SocialDanceVocabulary Vocab() => SocialDanceVocabulary.LoadDefault();

    [Fact]
    public void Project_maps_codes_to_dimensions_and_keeps_scalar_captures()
    {
        var rule = new PathRule("swing/!Stojan/*.mp3", "%artist% - %title% <tail>",
            [new("LIN", "dances", "Lindy Hop"), new("BAL", "dances", "Balboa")],
            Rating: new PlusRunRating());
        var ingest = new PathIngest(Vocab(), [rule]);

        PathProjection proj = ingest.Project("swing/!Stojan/Bob Crosby - Ja-Da BAL LIN ++++++.mp3")!;

        Assert.Equal(["Balboa", "Lindy Hop"], proj.Projected.Projected["dances"].OrderBy(x => x));
        Assert.Equal(["Bob Crosby"], proj.Projected.Projected["artist"]);
        Assert.Equal(["6"], proj.Projected.Projected["rating"]);
        Assert.False(proj.Projected.Projected.ContainsKey("path")); // source dropped after projection
    }

    [Fact]
    public void Project_returns_null_when_no_rule_matches()
        => Assert.Null(new PathIngest(Vocab(), []).Project("x/y.mp3"));

    [Fact]
    public void Validator_flags_a_from_path_code_whose_dimension_target_does_not_resolve()
    {
        var profile = new PersonalProfile(fromPath:
        [
            new PathRule("swing/*.mp3", "%artist% - %title% <tail>",
                [new("LIN", "dances", "Lindy Hop"), new("ZZZ", "dances", "Nonsense Dance")])
        ]);
        ValidationReport report = ProfileValidator.Validate(Vocab(), profile);
        Assert.Contains(report.Warnings, w => w.Contains("Nonsense Dance"));
        Assert.True(report.IsValid); // a dangling code is a warning (dropped), not a hard error
    }

    [Fact]
    public void Conformance_path_ingest_vector_reproduces()
    {
        using JsonDocument json = JsonDocument.Parse(
            System.IO.File.ReadAllText(ConformancePaths.Resolve("conformance/vectors/profile/path-ingest.json")));
        JsonElement ruleEl = json.RootElement.GetProperty("rule");
        var codes = ruleEl.GetProperty("codes").EnumerateObject()
            .SelectMany(c => c.Value.EnumerateObject().Select(d => new PathCode(c.Name, d.Name, d.Value.GetString()!)))
            .ToList();
        var rule = new PathRule(ruleEl.GetProperty("match").GetString()!, ruleEl.GetProperty("mask").GetString()!,
            codes, Rating: new PlusRunRating(), Year: new YearExtractor());
        var ingest = new PathIngest(Vocab(), [rule]);

        foreach (JsonElement c in json.RootElement.GetProperty("cases").EnumerateArray())
        {
            PathProjection proj = ingest.Project(c.GetProperty("path").GetString()!)!;
            foreach (JsonProperty exp in c.GetProperty("expect").EnumerateObject())
            {
                var got = proj.Projected.Projected[exp.Name].OrderBy(x => x);
                var want = exp.Value.EnumerateArray().Select(v => v.GetString()!).OrderBy(x => x);
                Assert.Equal(want, got);
            }
        }
    }
}
