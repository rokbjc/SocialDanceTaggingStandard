using SocialDanceTaggingStandard.Gates;
using SocialDanceTaggingStandard.Taxonomy.Ingest;

namespace SocialDanceTaggingStandard.Taxonomy.Profile;

/// <summary>Applies a <see cref="PersonalProfile"/> to a baseline vocabulary.</summary>
public static class ProfileExtensions
{
    /// <summary>
    /// Build a <see cref="CustomizedVocabulary"/> from a profile: validate it, apply the overlay (the
    /// baseline is never mutated — <see cref="SocialDanceVocabulary.With"/> returns a new instance), apply
    /// the color / short-label overrides, and construct the read-time <see cref="IngestMapper"/> over the
    /// cross-dimension rules whose target resolves (a dangling rule is dropped and reported as a warning).
    /// </summary>
    /// <remarks>Never throws on a bad profile: an overlay edit whose token collides is dropped and
    /// reported as an error in <see cref="CustomizedVocabulary.Report"/>, the same way a dangling
    /// cross-dimension rule is dropped and warned about.</remarks>
    public static CustomizedVocabulary WithProfile(this SocialDanceVocabulary vocab, PersonalProfile profile)
    {
        ArgumentNullException.ThrowIfNull(vocab);
        ArgumentNullException.ThrowIfNull(profile);

        var (report, overlaid) = ProfileValidator.Check(vocab, profile);
        var customized = ApplyColors(overlaid, profile.Colors);
        var rules = profile.CrossDimension.Where(r => RuleUsable(customized, r)).ToList();
        return new CustomizedVocabulary(customized, new IngestMapper(customized, rules), report);
    }

    /// <summary>
    /// Builds a <see cref="GateKeyResolver"/> from the profile's <see cref="PersonalProfile.GateAliases"/> — an
    /// ordinal wire-token ⇒ <see cref="FieldId"/> lookup for <c>ConfirmationGates.Parse</c>. A token with no
    /// alias yields null, so the parser falls through to the canonical field id (then preserve-and-flag). An
    /// empty profile yields a resolver that recognizes nothing (canonical-only parsing).
    /// </summary>
    public static GateKeyResolver GateResolver(this PersonalProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (profile.GateAliases.Count == 0) return static _ => null;
        var map = new Dictionary<string, FieldId>(StringComparer.Ordinal);
        foreach (var alias in profile.GateAliases) map[alias.Token] = new FieldId(alias.Field);
        return token => map.TryGetValue(token, out var field) ? field : null;
    }

    private static bool RuleUsable(SocialDanceVocabulary vocab, CrossDimensionRule r)
    {
        if (string.Equals(r.FromDimension, r.ToDimension, StringComparison.OrdinalIgnoreCase))
        {
            return false; // same-dimension remap is an error — use an alias
        }
        var dim = ProfileValidator.DimensionFor(vocab, new DimensionId(r.ToDimension));
        return dim is null || dim.Resolve(r.ToValue) is not null;   // non-vocabulary field target accepted as-is
    }

    private static SocialDanceVocabulary ApplyColors(SocialDanceVocabulary vocab, IReadOnlyList<ColorOverride> colors)
    {
        if (colors.Count == 0) return vocab;

        var byDim = colors
            .GroupBy(c => c.Dimension, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ColorOverride>)[.. g], StringComparer.OrdinalIgnoreCase);

        var core = new Dictionary<DimensionId, Dimension>();
        foreach (var kv in vocab.Core)
        {
            core[kv.Key] = byDim.TryGetValue(kv.Key.Value, out var os) ? Recolor(kv.Value, os) : kv.Value;
        }

        var danceOverrides = byDim.GetValueOrDefault("dances");
        var packs = new Dictionary<PackId, DancePack>();
        foreach (var kv in vocab.Packs)
        {
            var p = kv.Value;
            packs[kv.Key] = danceOverrides is null
                ? p
                : new DancePack(p.Name, p.DisplayName, p.Description, p.Families, Recolor(p.Dances, danceOverrides),
                    p.ReviewStatus);
        }

        return new SocialDanceVocabulary(vocab.Version, core, packs);
    }

    private static Dimension Recolor(Dimension dim, IReadOnlyList<ColorOverride> overrides)
    {
        var byId = new Dictionary<string, ColorOverride>(StringComparer.OrdinalIgnoreCase);
        foreach (var o in overrides)
        {
            var t = dim.Resolve(o.Value);
            if (t is not null) byId[t.Id] = o;   // last override for a given term wins
        }
        if (byId.Count == 0) return dim;

        var terms = new List<VocabularyTerm>(dim.Terms.Count);
        foreach (var t in dim.Terms)
        {
            if (byId.TryGetValue(t.Id, out var o))
            {
                var d = t.Display;
                terms.Add(t.WithDisplay(new TermDisplay(
                    d?.Code, d?.Emoji, d?.Icon, o.Color ?? d?.Color, o.Label ?? d?.ShortLabel)));
            }
            else
            {
                terms.Add(t);
            }
        }
        return new Dimension(dim.Name, dim.Layer, terms, dim.AttributeReview, dim.Grouping);
    }
}
