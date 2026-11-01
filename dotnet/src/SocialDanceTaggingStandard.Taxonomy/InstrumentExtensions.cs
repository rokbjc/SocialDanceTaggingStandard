namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>Helpers over the unified instrument vocabulary (the <c>instrument</c> dimension, which carries
/// both track-lead and credit-only instruments). The <c>instrument_led</c> lead field draws the <see
/// cref="IsLeadCapable"/> subset; TMCL credit resolution draws the whole set.</summary>
public static class InstrumentExtensions
{
    /// <summary>True unless the term is explicitly credit-only (<c>lead_capable: false</c>). An absent
    /// flag means lead-capable, so this is the correct predicate for the instrument_led lead subset.</summary>
    public static bool IsLeadCapable(this Instrument term) => term.LeadCapable != false;
}
