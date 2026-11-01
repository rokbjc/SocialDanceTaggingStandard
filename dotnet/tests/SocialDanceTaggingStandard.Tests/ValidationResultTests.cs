using SocialDanceTaggingStandard.Validation;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

public sealed class ValidationResultTests
{
    [Fact]
    public void Valid_HasNoError()
    {
        FieldValidationResult r = FieldValidationResult.Valid;
        Assert.True(r.IsValid);
        Assert.Null(r.Error);
    }

    [Fact]
    public void Default_StructValue_IsValid()
    {
        FieldValidationResult r = default;
        Assert.True(r.IsValid);
        Assert.Null(r.Error);
    }

    [Fact]
    public void Fail_IsInvalidAndCarriesTheError()
    {
        FieldValidationResult r = FieldValidationResult.Fail(ValidationError.BadGrade);
        Assert.False(r.IsValid);
        Assert.Equal(ValidationError.BadGrade, r.Error);
    }

    [Theory]
    [InlineData(ValidationError.NotInteger, "not_integer")]
    [InlineData(ValidationError.OutOfRange, "out_of_range")]
    [InlineData(ValidationError.NotInEnum, "not_in_enum")]
    [InlineData(ValidationError.BadGrade, "bad_grade")]
    [InlineData(ValidationError.NoMatch, "no_match")]
    public void ToWireCode_MapsEachMemberToItsSnakeCaseString(ValidationError error, string expected)
        => Assert.Equal(expected, error.ToWireCode());

    [Fact]
    public void ToWireCode_IsTotalOverEveryDefinedMember()
    {
        foreach (ValidationError error in Enum.GetValues<ValidationError>())
        {
            _ = error.ToWireCode(); // must not throw for any defined member
        }
    }
}
