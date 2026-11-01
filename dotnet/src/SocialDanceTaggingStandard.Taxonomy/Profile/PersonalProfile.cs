using SocialDanceTaggingStandard.Taxonomy.Ingest;
using SocialDanceTaggingStandard.Validation;

namespace SocialDanceTaggingStandard.Taxonomy.Profile;

/// <summary>How a mapping rule compares the source token to its <c>FromValue</c>.</summary>
public enum MatchMode
{
    /// <summary>Case-sensitive ordinal match.</summary>
    Exact,
    /// <summary>Case-insensitive ordinal match (the default).</summary>
    CaseInsensitive,
}

/// <summary>
/// A per-DJ display override for one <c>(Dimension, Value)</c>: a chip <see cref="Color"/> and/or a
/// short <see cref="Label"/>. Presentation only — never hashed and unreachable from the write path
/// (a "rename" is just a <see cref="Label"/> override; the canonical never moves).
/// </summary>
public sealed record ColorOverride(string Dimension, string Value, string? Color = null, string? Label = null);

/// <summary>
/// A cross-dimension ingest rule: a value in <paramref name="FromDimension"/> means a value in
/// <paramref name="ToDimension"/> (e.g. genre <c>"Slovenian"</c> → artist_country <c>"SI"</c>). Resolved
/// strictly ABOVE the sync layer by <see cref="IngestMapper"/>, so it never alters a canonical that reaches
/// the stable hash. Same-dimension remaps are NOT modelled here — they fold into the overlay's aliases.
/// </summary>
public sealed record CrossDimensionRule(
    string FromDimension,
    string FromValue,
    string ToDimension,
    string ToValue,
    bool KeepSource = true,
    bool Migrate = false,
    MatchMode Match = MatchMode.CaseInsensitive);

/// <summary>
/// A confirmation-gate wire-token alias: the raw token <paramref name="Token"/> as written in the gate tag
/// (e.g. a legacy MM5 catalog's <c>Genres</c>, or <c>Occasions</c>) means the SDTS field
/// <paramref name="Field"/> (its <c>FieldId.Value</c>, e.g. <c>genre</c> / <c>dances</c>). Read-time only;
/// never touches a canonical value or the stable hash. Consumed by <see cref="ProfileExtensions.GateResolver"/>,
/// which feeds <c>SocialDanceTaggingStandard.Gates.ConfirmationGates.Parse</c>.
/// </summary>
public sealed record GateAlias(string Token, string Field);

/// <summary>
/// An OPTIONAL, NON-NORMATIVE per-DJ customization layer over the immutable baseline vocabulary:
/// a vocabulary <see cref="Overlay"/> (add/remove terms and aliases), presentation <see cref="Colors"/>
/// (color / short-label overrides), and <see cref="CrossDimension"/> ingest rules. Apply it with
/// <see cref="ProfileExtensions.WithProfile"/>; the baseline is never mutated and the stable hash is
/// never affected (see <c>docs/standard/profile.md</c> §4).
/// </summary>
public sealed class PersonalProfile
{
    /// <summary>Creates a personal profile (all sections optional).</summary>
    public PersonalProfile(
        VocabularyOverlay? overlay = null,
        IReadOnlyList<ColorOverride>? colors = null,
        IReadOnlyList<CrossDimensionRule>? crossDimension = null,
        string? owner = null,
        IReadOnlyList<PathRule>? fromPath = null,
        IReadOnlyList<GateAlias>? gateAliases = null)
    {
        Overlay = overlay ?? new VocabularyOverlay();
        Colors = colors ?? [];
        CrossDimension = crossDimension ?? [];
        Owner = owner;
        FromPath = fromPath ?? [];
        GateAliases = gateAliases ?? [];
    }

    /// <summary>Add/remove terms and aliases over the baseline (the existing deployment-overlay model;
    /// same-dimension remaps live here as added aliases).</summary>
    public VocabularyOverlay Overlay { get; }

    /// <summary>Per-<c>(dimension, value)</c> color / short-label overrides (presentation only).</summary>
    public IReadOnlyList<ColorOverride> Colors { get; }

    /// <summary>Cross-dimension ingest rules (a value in one dimension projects onto another).</summary>
    public IReadOnlyList<CrossDimensionRule> CrossDimension { get; }

    /// <summary>Filename/path ingest rules (read-time projection of names/paths onto SDTS).</summary>
    public IReadOnlyList<PathRule> FromPath { get; }

    /// <summary>Confirmation-gate wire-token ⇒ field aliases (read-time; legacy/extension gate spellings).</summary>
    public IReadOnlyList<GateAlias> GateAliases { get; }

    /// <summary>The profile owner id (advisory; never normative).</summary>
    public string? Owner { get; }
}

/// <summary>
/// The result of applying a <see cref="PersonalProfile"/>: a NEW customized <see cref="Vocabulary"/>
/// (the baseline is untouched), the read-time <see cref="Mapper"/>, and the validation <see cref="Report"/>.
/// </summary>
public sealed record CustomizedVocabulary(
    SocialDanceVocabulary Vocabulary,
    IngestMapper Mapper,
    ValidationReport Report);
