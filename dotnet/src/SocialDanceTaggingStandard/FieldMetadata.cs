namespace SocialDanceTaggingStandard;

/// <summary>
/// The human-facing description of an SDTS field (data/fields.yaml) at RUNTIME — the display name and
/// prose the spec attaches to a field, reachable through a <see cref="FieldId"/> without reading the
/// YAML source. The per-field values live in the generated <see cref="FieldCatalog"/>; the strongly-typed
/// ids and the XML doc comments come from the same source, so a doc comment and this record never diverge.
/// </summary>
/// <param name="Id">The field this describes.</param>
/// <param name="DisplayName">The field's human label (e.g. <c>"Rhythmic Feel"</c>).</param>
/// <param name="Description">The full spec description — "what SDTS says a field is".</param>
/// <param name="DjUse">The one-line DJ-workflow note, or null where the field defines none.</param>
/// <param name="Group">The field's catalog group id (e.g. <c>"taxonomy-core-a"</c>).</param>
/// <param name="Layer">The field's layer (<c>taxonomy</c> / <c>measured</c> / <c>derived</c> / …).</param>
/// <param name="BackingDimension">For a vocabulary-backed field, the id of the vocabulary dimension
/// whose values it draws from — <c>dances</c> for <c>dance</c>, <c>instrument</c> for <c>instrument_led</c>,
/// its own id otherwise. Null for a field with no vocabulary dimension (a number, a cue, a gate). It is a
/// bare string here because the core package does not depend on the taxonomy's <c>DimensionId</c> type;
/// a taxonomy consumer wraps it.</param>
public sealed record FieldMetadata(
    FieldId Id,
    string DisplayName,
    string Description,
    string? DjUse,
    string Group,
    string Layer,
    string? BackingDimension);
