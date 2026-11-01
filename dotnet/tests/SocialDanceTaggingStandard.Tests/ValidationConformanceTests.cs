using System.Text.Json;
using SocialDanceTaggingStandard.TestKit;
using SocialDanceTaggingStandard.Validation;
using Xunit;

namespace SocialDanceTaggingStandard.Tests;

/// <summary>
/// Runs the validation conformance vectors (<c>conformance/vectors/validation/fields.json</c>) against the
/// .NET <see cref="FieldValidator"/> — the SAME file the Python validator consumes.
/// </summary>
public sealed class ValidationConformanceTests
{
    private static readonly JsonSerializerOptions SnakeCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public static IEnumerable<object?[]> Cases()
    {
        string path = Path.Combine(ConformancePaths.VectorsDir, "validation", "fields.json");
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(path));
        foreach (JsonProperty field in doc.RootElement.GetProperty("fields").EnumerateObject())
        {
            string constraintsJson = field.Value.GetProperty("constraints").GetRawText();
            foreach (JsonElement c in field.Value.GetProperty("cases").EnumerateArray())
            {
                JsonElement err = c.GetProperty("error");
                yield return
                [
                    field.Name,
                    c.GetProperty("value").GetString(),
                    c.GetProperty("valid").GetBoolean(),
                    err.ValueKind == JsonValueKind.Null ? null : err.GetString(),
                    constraintsJson
                ];
            }
        }
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Validate_MatchesVectors(
        string field, string? value, bool expectedValid, string? expectedError, string constraintsJson)
    {
        _ = field;
        FieldConstraints constraints = JsonSerializer.Deserialize<FieldConstraints>(constraintsJson, SnakeCase)!;

        FieldValidationResult result = FieldValidator.Validate(value, constraints);

        Assert.Equal(expectedValid, result.IsValid);
        if (!expectedValid)
        {
            Assert.Equal(expectedError, result.Error?.ToWireCode());
        }
    }
}
