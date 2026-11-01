using SocialDanceTaggingStandard.Taxonomy;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>The dance-vocabulary gap decisions (songoulash-2026-09-25): which blues idioms became values and
/// which did not, the widened Blues band, the two separate-scene packs, and the routine dimension.</summary>
public sealed class DanceVocabularyGapsTests
{
    private static readonly SocialDanceVocabulary Vocab = SocialDanceVocabulary.LoadDefault();

    [Theory]
    [InlineData("Struttin'")]
    [InlineData("Savoy Walk")]
    [InlineData("Texas Shuffle")]
    [InlineData("Chicago Triple")]
    [InlineData("Piedmont Triple")]
    [InlineData("Funky Butt")]
    public void BluesIdiom_IsABluesPackValue_WithNoInventedTempo(string idiom)
    {
        var term = Vocab.Packs[PackIds.Blues].Dances.Resolve(idiom);

        Assert.NotNull(term);
        Assert.Equal("blues-idiom", term!.Family);
        // No blues-dance source gives an idiom a BPM range, so none is stored.
        var typed = Dances.Blues.All.Single(d => d.Canonical == idiom);
        Assert.Null(typed.Tempo);
    }

    [Theory]
    [InlineData("Grind")]
    [InlineData("Fishtail")]
    public void JookinMovement_IsNotADanceValue(string movement)
        // Sources treat these as movement vocabulary inside jookin', with no music profile of their own.
        => Assert.Null(Vocab.Dances([.. Vocab.Packs.Keys]).Resolve(movement));

    [Fact]
    public void Struttin_DoesNotClaimTheNewOrleansStrut()
        => Assert.Null(Vocab.Packs[PackIds.Blues].Dances.Resolve("Strut"));

    [Fact]
    public void BluesBand_CoversFastBlues()
        => Assert.Equal(new BpmRange(50, 160), Dances.Blues.BluesDancing.Tempo);

    [Fact]
    public void WestCoastSwingAndCarolinaShag_AreOptInPacks_ResolvableByCode()
    {
        Assert.Equal("West Coast Swing", Vocab.Packs[PackIds.WestCoastSwing].Dances.Resolve("WCS")!.Canonical);
        Assert.Equal("Carolina Shag", Vocab.Packs[PackIds.CarolinaShag].Dances.Resolve("CAS")!.Canonical);

        // Not in the swing pack: a swing+blues library that never enables these packs never sees them.
        Assert.Null(Vocab.Packs[PackIds.Swing].Dances.Resolve("West Coast Swing"));
        Assert.Null(Vocab.Packs[PackIds.Swing].Dances.Resolve("Carolina Shag"));
    }

    [Fact]
    public void CarolinaShag_IsDistinctFromTheSwingEraShags()
    {
        var dances = Vocab.Dances([.. Vocab.Packs.Keys]);

        var carolina = dances.Resolve("Carolina Shag")!;
        Assert.NotEqual(dances.Resolve("Collegiate Shag")!.Id, carolina.Id);
        Assert.NotEqual(dances.Resolve("St. Louis Shag")!.Id, carolina.Id);
    }

    [Fact]
    public void WestCoastBlues_IsABluesChildGenre()
    {
        var genre = Vocab.Dimension(DimensionIds.Genre)!.Resolve("West Coast Blues");

        Assert.NotNull(genre);
        Assert.Equal("blues", Genres.WestCoastBlues.Parent);
    }

    [Theory]
    [InlineData("Shim Sham")]
    [InlineData("ShimSham")]
    [InlineData("SHS")]
    public void Routine_ResolvesCanonicalSpacelessAndCode_ToOneTerm(string token)
        => Assert.Equal("Shim Sham", Vocab.Dimension(DimensionIds.Routine)!.Resolve(token)!.Canonical);

    [Fact]
    public void RoutineField_IsBackedByTheRoutineDimension()
        => Assert.Equal(DimensionIds.Routine.Value, SocialDanceTaggingStandard.FieldCatalog.For(FieldIds.Routine)!.BackingDimension);

    [Fact]
    public void EveryRoutine_PointsAtRealDances()
    {
        var dances = Vocab.Dances([.. Vocab.Packs.Keys]);
        foreach (var routine in Routines.All)
        {
            Assert.NotEmpty(routine.TypicalDances);
            foreach (var dance in routine.TypicalDances)
            {
                Assert.NotNull(dances.Resolve(dance));
            }
        }
    }
}
