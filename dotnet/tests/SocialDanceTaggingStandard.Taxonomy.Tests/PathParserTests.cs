using SocialDanceTaggingStandard.Taxonomy.Ingest;
using Xunit;
using PathParser = SocialDanceTaggingStandard.Taxonomy.Ingest.PathParser;

namespace SocialDanceTaggingStandard.Tests;

public sealed class PathParserTests
{
    private static PathRule Rule(string match = "swing/!Stojan/*.mp3", string mask = "%artist% - %title% <tail>")
        => new(match, mask, []);

    [Theory]
    [InlineData("swing/!Stojan/Bob Crosby - Ja-Da BAL LIN.mp3", true)]
    [InlineData("swing/!Stojan/Stari Mački/01 Title.mp3", false)] // deeper folder, not a direct child
    [InlineData("blues/other/Track.mp3", false)]
    public void Matches_respects_the_path_glob(string relativePath, bool expected)
        => Assert.Equal(expected, PathParser.Matches(Rule(), relativePath));

    [Fact]
    public void Parse_splits_artist_title_peels_codes_rating_and_year()
    {
        var rule = new PathRule(
            Match: "swing/!Stojan/*.mp3",
            Mask: "%artist% - %title% <tail>",
            Codes: [new("LIN", "dances", "Lindy Hop"), new("BAL", "dances", "Balboa")],
            Rating: new PlusRunRating(),
            Year: new YearExtractor());

        PathCapture cap = PathParser.Parse("swing/!Stojan/1953 Bob Crosby - Ja-Da BAL LIN ++++++.mp3", rule)!;

        Assert.Equal(["Bob Crosby"], cap.Fields["artist"]);
        Assert.Equal(["Ja-Da"], cap.Fields["title"]);
        Assert.Equal(["BAL", "LIN"], cap.Fields["path"]);   // recognised tail, original order
        Assert.Equal(["6"], cap.Fields["rating"]);          // +++++ + = 6
        Assert.Equal(["1953"], cap.Fields["recording_year"]);
        Assert.Empty(cap.Unmapped);
    }

    [Fact]
    public void Parse_stops_peeling_at_an_unrecognised_token_and_keeps_it_in_title()
    {
        var rule = new PathRule("swing/!Stojan/*.mp3", "%artist% - %title% <tail>",
            [new("PIANO", "instrument_led", "Piano"), new("LIN", "dances", "Lindy Hop"), new("CH", "dances", "Charleston")]);
        PathCapture cap = PathParser.Parse("swing/!Stojan/Boardwalk OST - Harlem Strut PIANO LIN CH.mp3", rule)!;
        Assert.Equal(["Harlem Strut"], cap.Fields["title"]);
        Assert.Equal(["PIANO", "LIN", "CH"], cap.Fields["path"]);
    }

    [Fact]
    public void Parse_flags_unrecognised_trailing_token_run_when_OnUnknownToken_is_Flag()
    {
        // No codes mapped yet: every trailing short token is a candidate to audit, none guessed.
        var rule = new PathRule("swing/!Stojan/*.mp3", "%artist% - %title% <tail>", []);
        PathCapture cap = PathParser.Parse("swing/!Stojan/Dean Martin - Chee Chee LIN 6K.mp3", rule)!;
        Assert.Contains("6K", cap.Unmapped);
        Assert.Contains("LIN", cap.Unmapped);
    }

    [Fact]
    public void Parse_returns_null_when_the_glob_does_not_match()
        => Assert.Null(PathParser.Parse("blues/x.mp3", new PathRule("swing/*.mp3", "%title%", [])));
}
