namespace SocialDanceTaggingStandard.Taxonomy;

/// <summary>
/// Whether SDTS flags vocabulary data as awaiting confirmation by a domain expert — the <c>validation</c>
/// status of a pack (<see cref="DancePack.ReviewStatus"/>) or of a term attribute
/// (<see cref="Dimension.AttributeReview"/>).
/// </summary>
public enum ReviewStatus
{
    /// <summary>No review flag.</summary>
    Unflagged,

    /// <summary><c>expert-review-pending</c>: an SDTS-authored draft a domain expert has not confirmed yet.</summary>
    ExpertReviewPending,
}
