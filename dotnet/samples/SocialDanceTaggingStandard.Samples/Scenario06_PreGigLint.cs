using SocialDanceTaggingStandard.Taxonomy;
using SocialDanceTaggingStandard.Validation;

namespace SocialDanceTaggingStandard.Samples;

/// <summary>
/// Scenario 6 — a pre-gig lint pass. FieldValidator catches malformed SHAPE (out-of-range bpm, bad
/// inline grade, malformed gates); it deliberately does NOT check vocabulary membership for open enum
/// fields, so a second pass through the taxonomy canonicalizes DJ shorthand and flags values that no
/// alias resolves.
/// </summary>
internal static class PreGigLint
{
    public static void Run()
    {
        // Constraints are hand-built here (the Validation subsystem ships no loader/registry);
        // they mirror data/field-constraints.yaml.
        var constraints = new Dictionary<string, FieldConstraints>
        {
            ["bpm"] = new IntConstraint { Min = 1, Max = 330 },
            ["dances"] = new EnumConstraint
            {
                Cardinality = "multi",
                Grades = ["core", "edge", "fusion"],
                AllowValidationMarker = true,
            },
            ["confirmation_gates"] = new CompositeConstraint
            {
                Cardinality = "multi",
                // Patterns are stored unanchored; FieldValidator wraps them as ^(?:...)$.
                Pattern = "[A-Za-z][A-Za-z0-9]*:[0-9]+(:[0-9a-f]{4})?",
            },
        };

        var track = new Dictionary<string, string?>
        {
            ["bpm"] = "400",                              // out_of_range
            ["dances"] = "#LH; Blues:traditional",        // alias to canonicalize + bad_grade
            ["confirmation_gates"] = "BPM; Genres:1",     // "BPM" alone -> no_match
        };

        Console.WriteLine("  shape violations:");
        foreach ((string field, FieldConstraints c) in constraints)
        {
            if (!track.TryGetValue(field, out string? raw))
            {
                continue;
            }

            FieldValidationResult result = FieldValidator.Validate(raw, c);
            if (!result.IsValid)
            {
                Console.WriteLine($"    {field}=\"{raw}\" -> {result.Error?.ToWireCode()}");
            }
        }

        Console.WriteLine("  vocabulary / alias pass (FieldValidator skips membership for open enums):");
        var vocab = SocialDanceVocabulary.LoadDefault();
        Dimension dances = vocab.Dances(PackIds.Swing, PackIds.Blues);
        foreach (string token in (track["dances"] ?? string.Empty).Split(["; "], StringSplitOptions.None))
        {
            VocabularyTerm? term = dances.Resolve(token);   // strips :grade / trailing '?'
            if (term is null)
            {
                Console.WriteLine($"    dances \"{token}\" -> needs_manual_fix (no alias resolves; no fuzzy match)");
            }
            else if (!string.Equals(token, term.Canonical, StringComparison.Ordinal))
            {
                Console.WriteLine($"    dances \"{token}\" -> canonicalize to \"{term.Canonical}\"");
            }
        }
    }
}
