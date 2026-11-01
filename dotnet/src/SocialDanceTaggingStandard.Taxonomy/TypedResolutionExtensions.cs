namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// Opt-in string→typed-term resolution over a loaded <see cref="SocialDanceVocabulary"/>. A consumer
/// glues the storage sibling (<c>SocialDanceTaggingStandard.Id3</c>'s <c>SdtsFile.Genres</c>/<c>Moods</c>/<c>Dances</c>, which
/// return <see cref="IReadOnlyList{String}"/>) to this semantic layer at the call site —
/// e.g. <c>vocab.ResolveDances([.. file.Dances])</c>. This keeps <c>SocialDanceTaggingStandard.Id3</c> vocabulary-free.
/// <para>
/// The matching engine is never re-implemented here: every method funnels through the generated
/// <see cref="VocabularyViews"/> resolvers (<c>vocab.Genre()</c>/<c>.Mood()</c>/<c>.Dance(packs)</c>) →
/// <c>*View.Resolve</c>. Default policy is <b>keep-unknown</b> (lossless — a miss surfaces as a generic
/// term carrying its raw text); the <c>*Known</c> variants drop misses.
/// </para>
/// </summary>
public static class TypedResolutionExtensions
{
    extension(SocialDanceVocabulary vocab)
    {
        // ── genre ────────────────────────────────────────────────────────────────
        /// <summary>Resolve each token to a <see cref="GenreValue"/>, keeping unknowns (lossless).</summary>
        public IReadOnlyList<GenreValue> ResolveGenres(IEnumerable<string> values)
        {
            var view = vocab.Genre();
            return [.. values.Select(view.Resolve)];
        }

        /// <summary>Resolve to canonical strings, keeping unknowns as their stored text (lossless).</summary>
        public IReadOnlyList<string> ResolveGenreValues(IEnumerable<string> values)
            => [.. vocab.ResolveGenres(values).Select(v => v.Canonical)];

        /// <summary>Resolve, dropping tokens that do not match the catalog.</summary>
        public IReadOnlyList<GenreValue> ResolveKnownGenres(IEnumerable<string> values)
            => [.. vocab.ResolveGenres(values).Where(v => v.IsKnown)];

        // ── mood ─────────────────────────────────────────────────────────────────
        /// <summary>Resolve each token to a <see cref="MoodValue"/>, keeping unknowns (lossless).</summary>
        public IReadOnlyList<MoodValue> ResolveMoods(IEnumerable<string> values)
        {
            var view = vocab.Mood();
            return [.. values.Select(view.Resolve)];
        }

        /// <summary>Resolve to canonical strings, keeping unknowns as their stored text (lossless).</summary>
        public IReadOnlyList<string> ResolveMoodValues(IEnumerable<string> values)
            => [.. vocab.ResolveMoods(values).Select(v => v.Canonical)];

        /// <summary>Resolve, dropping tokens that do not match the catalog.</summary>
        public IReadOnlyList<MoodValue> ResolveKnownMoods(IEnumerable<string> values)
            => [.. vocab.ResolveMoods(values).Where(v => v.IsKnown)];

        // ── dances ───────────────────────────────────────────────────────────────
        /// <summary>Resolve each token to a <see cref="DanceValue"/> across the named
        /// <paramref name="packs"/> (default: every enabled pack), keeping unknowns (lossless).</summary>
        public IReadOnlyList<DanceValue> ResolveDances(IEnumerable<string> values, params PackId[] packs)
        {
            var view = vocab.Dance(packs);
            return [.. values.Select(view.Resolve)];
        }

        /// <summary>Resolve to canonical strings across the named <paramref name="packs"/>, keeping
        /// unknowns as their stored text (lossless).</summary>
        public IReadOnlyList<string> ResolveDanceValues(IEnumerable<string> values, params PackId[] packs)
            => [.. vocab.ResolveDances(values, packs).Select(v => v.Canonical)];

        /// <summary>Resolve across the named <paramref name="packs"/>, dropping tokens that do not
        /// match the catalog.</summary>
        public IReadOnlyList<DanceValue> ResolveKnownDances(IEnumerable<string> values, params PackId[] packs)
            => [.. vocab.ResolveDances(values, packs).Where(v => v.IsKnown)];
    }
}
