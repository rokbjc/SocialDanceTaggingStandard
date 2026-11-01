using System.Text.RegularExpressions;
using SocialDanceTaggingStandard.Validation;

namespace SocialDanceTaggingStandard.Taxonomy.Profile;

/// <summary>Identity + integrity governance for a <see cref="PersonalProfile"/> (<c>docs/standard/profile.md</c> §8).</summary>
public static class ProfileValidator
{
    private static readonly Regex CustomId = new("^(x|user)-[a-z0-9][a-z0-9-]*$", RegexOptions.Compiled);

    /// <summary>Validate <paramref name="profile"/> against the baseline <paramref name="vocab"/>.</summary>
    public static ValidationReport Validate(SocialDanceVocabulary vocab, PersonalProfile profile)
        => Check(vocab, profile).Report;

    /// <summary>
    /// Validate the profile AND apply what survives: the report plus the overlaid vocabulary. An edit
    /// whose token collides with an existing one is dropped and reported, so a profile with one bad
    /// alias still takes effect elsewhere instead of failing whole.
    /// </summary>
    internal static (ValidationReport Report, SocialDanceVocabulary Overlaid) Check(
        SocialDanceVocabulary vocab, PersonalProfile profile)
    {
        var messages = new List<ValidationMessage>();

        // 1. Custom term ids must be user-namespaced; a custom canonical must not shadow a baseline term.
        foreach (var entry in profile.Overlay.AddTerms)
        {
            // The dances dimension is composed of per-pack sets, and the overlay engine cannot add a
            // term to it (add a custom pack instead). Reject here so a profile can't validate-but-no-op.
            if (entry.Key == SocialDanceVocabulary.DancesDimension)
            {
                AddError("custom terms cannot be added to the 'dances' dimension (it is composed of per-pack dance sets) — add a custom dance pack instead");
                continue;
            }
            var dim = DimensionFor(vocab, entry.Key);
            foreach (var t in entry.Value)
            {
                if (!CustomId.IsMatch(t.Id))
                {
                    AddError($"custom term id '{t.Id}' in '{entry.Key}' must match ^(x|user)-[a-z0-9][a-z0-9-]*$");
                }

                if (dim?.Resolve(t.Canonical) is not null)
                {
                    AddError(
                        $"custom term canonical '{t.Canonical}' in '{entry.Key}' collides with a baseline term/alias");
                }
            }
        }

        // 2. Alias-uniqueness: an added alias must not already resolve to a DIFFERENT canonical.
        foreach (var entry in profile.Overlay.AddAliases)
        {
            var dim = DimensionFor(vocab, entry.Key);
            if (dim is null) continue;
            foreach (var perTerm in entry.Value)
            {
                var target = dim.Resolve(perTerm.Key);
                foreach (var alias in perTerm.Value)
                {
                    var existing = dim.Resolve(alias);
                    if (existing is not null && target is not null
                        && !string.Equals(existing.Id, target.Id, StringComparison.OrdinalIgnoreCase))
                        AddError($"alias '{alias}' in '{entry.Key}' already resolves to '{existing.Canonical}'; cannot also map to '{target.Canonical}'");
                }
            }
        }

        // 3. A cross-dimension remap whose VOCABULARY target doesn't resolve is dropped (warning). A target
        //    in a non-vocabulary field (e.g. artist_country) is a free-form canonical write and accepted.
        foreach (var r in profile.CrossDimension)
        {
            if (string.Equals(r.FromDimension, r.ToDimension, StringComparison.OrdinalIgnoreCase))
            {
                AddError(
                    $"cross-dimension rule '{r.FromDimension}:{r.FromValue}' targets its own dimension — fold a same-dimension remap into add_aliases");
            }
            var dim = DimensionFor(vocab, new DimensionId(r.ToDimension));
            if (dim is not null && dim.Resolve(r.ToValue) is null)
            {
                AddWarning(
                    $"cross-dimension remap target '{r.ToDimension}:{r.ToValue}' does not resolve — rule dropped");
            }
        }

        // 4. from_path codes: a code targeting a vocabulary dimension whose value doesn't resolve is dropped
        //    (warning), mirroring the cross-dimension rule. A code's source dimension is always "path".
        foreach (var rule in profile.FromPath)
        {
            foreach (var code in rule.Codes)
            {
                var dim = DimensionFor(vocab, new DimensionId(code.ToDimension));
                if (dim is not null && dim.Resolve(code.ToValue) is null)
                {
                    AddWarning(
                        $"from_path code '{code.Token}' → '{code.ToDimension}:{code.ToValue}' does not resolve — dropped");
                }
            }
        }

        var overlaid = ApplySurvivingEdits(vocab, profile.Overlay, messages);
        return (new ValidationReport(messages), overlaid);

        void AddError(string text) => messages.Add(new ValidationMessage(ValidationSeverity.Error, text));

        void AddWarning(string text) => messages.Add(new ValidationMessage(ValidationSeverity.Warning, text));
    }

    /// <summary>
    /// Apply the overlay, dropping the edits that collide. <see cref="Dimension"/> is the only judge of
    /// what collides — this asks it rather than restating the rule, so the two cannot drift apart.
    /// </summary>
    private static SocialDanceVocabulary ApplySurvivingEdits(
        SocialDanceVocabulary vocab, VocabularyOverlay overlay, List<ValidationMessage> messages)
    {
        try
        {
            return vocab.With(overlay);
        }
        catch (SdtsVocabularyException)
        {
            // At least one edit collides; the retry below finds which and keeps the rest.
        }

        var (removals, additions) = overlay.Split();
        var applied = vocab.With(removals);
        foreach (var (what, edit) in additions)
        {
            try
            {
                applied = applied.With(edit);
            }
            catch (SdtsVocabularyException ex)
            {
                messages.Add(new ValidationMessage(
                    ValidationSeverity.Error, $"{what} was dropped — {ex.Message}"));
            }
        }
        return applied;
    }

    /// <summary>The runtime dimension for a field/dimension id: a core dimension, or the combined dances
    /// dimension over all enabled packs for <c>dances</c>; null for a non-vocabulary field.</summary>
    internal static Dimension? DimensionFor(SocialDanceVocabulary vocab, DimensionId dimension)
        => dimension == SocialDanceVocabulary.DancesDimension
            ? (vocab.Packs.Count > 0 ? vocab.Dances([.. vocab.Packs.Keys]) : null)
            : vocab.Dimension(dimension);
}
