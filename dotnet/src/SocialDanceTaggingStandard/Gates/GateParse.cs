namespace SocialDanceTaggingStandard.Gates;

/// <summary>
/// Resolves a raw confirmation-gate wire token to the <see cref="FieldId"/> it names, or null if it does not
/// recognize it. The canonical token is a <see cref="FieldId.Value"/>; a resolver (typically built from a
/// personal profile) layers legacy/extension spellings on top — e.g. an MM5 catalog's <c>Genres</c> ⇒
/// <c>genre</c> or <c>Occasions</c> ⇒ <c>dances</c>. Returning null lets the parser fall through to the
/// canonical lookup and then preserve-and-flag, rather than guess.
/// </summary>
/// <param name="token">The raw wire token (the part before the first ':').</param>
/// <returns>The field the token names, or null if unknown to this resolver.</returns>
public delegate FieldId? GateKeyResolver(string token);

/// <summary>
/// The outcome of <see cref="ConfirmationGates.Parse"/>: the parsed <see cref="Gates"/>, plus the wire tokens
/// that were neither aliased nor a canonical field id (<see cref="UnrecognizedTokens"/>). Unrecognized tokens
/// are still preserved in <see cref="Gates"/> (no round-trip loss) — surface them as a warning so they can be
/// mapped by adding a profile alias.
/// </summary>
/// <param name="Gates">The parsed gate ledger (unknown tokens preserved as opaque field ids).</param>
/// <param name="UnrecognizedTokens">Wire tokens no resolver/canonical lookup recognized, in first-seen order.</param>
public sealed record GateParse(ConfirmationGates Gates, IReadOnlyList<string> UnrecognizedTokens)
{
    /// <summary>True when every wire token resolved to a known (aliased or canonical) field.</summary>
    public bool AllRecognized => UnrecognizedTokens.Count == 0;
}
