using SocialDanceTaggingStandard;
using SocialDanceTaggingStandard.Internal;   // StableHash
using SocialDanceTaggingStandard.Validation;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// Scenario 5 — a re-implementer (Rust/JS) pins their port to the CC0 conformance vectors. The sync
/// baseline IS a StableHash and the engine compares only by that hash, so a one-bit divergence across
/// languages silently mis-fires sync. These are the exact reference values from
/// conformance/vectors/hash.json and the dances/bpm constraints; a faithful port must reproduce them.
/// </summary>
internal static class Conformance
{
    public static void Run()
    {
        (string? Input, string Hash)[] hashVectors =
        [
            (null, "00000000"),
            ("", "00000000"),
            ("Lindy Hop", "41379818"),
            ("Lindy Hop; Balboa", "9e33e3a8"),
            ("Café", "cfc290a9"),            // multibyte: catches a UTF-16/codepoint port bug
            ("120", "733864ae"),
        ];

        int pass = 0;
        foreach ((string? input, string expected) in hashVectors)
        {
            string actual = StableHash.Of(input);
            bool ok = actual == expected;
            pass += ok ? 1 : 0;
            Console.WriteLine($"    stable_hash({(input ?? "<null>"),-20}) = {actual}  {(ok ? "OK" : $"FAIL (want {expected})")}");
        }
        Console.WriteLine($"  hash vectors: {pass}/{hashVectors.Length} match");

        var dances = new EnumConstraint
        {
            Cardinality = "multi",
            Grades = ["core", "edge", "fusion"],
            AllowValidationMarker = true,
        };
        var bpm = new IntConstraint { Min = 1, Max = 330 };

        Check("Lindy Hop:core?", dances);   // grade + needs-validation marker -> OK
        Check("Blues:traditional", dances); // -> bad_grade
        Check("186", bpm);                  // -> OK
        Check("400", bpm);                  // -> out_of_range
    }

    private static void Check(string value, FieldConstraints constraints)
    {
        FieldValidationResult result = FieldValidator.Validate(value, constraints);
        Console.WriteLine($"    validate(\"{value}\") -> {(result.IsValid ? "OK" : result.Error?.ToWireCode())}");
    }
}
